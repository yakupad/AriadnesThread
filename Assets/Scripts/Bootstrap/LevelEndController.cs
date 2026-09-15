using AriadnesThread.Audio;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Meta;
using AriadnesThread.Core.Tension;
using AriadnesThread.GameCenterIntegration;
using AriadnesThread.Meta;
using AriadnesThread.Player;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// Watches for win (reaching the exit) or loss (Caught), computes the Kristal reward,
    /// persists it, and submits the result to Game Center. Submission never blocks anything
    /// else — see GameCenterManager's fail-soft design.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LevelEndController : MonoBehaviour
    {
        private static readonly Color WinFlashColor = new Color(1f, 0.95f, 0.6f);
        private static readonly Color CaughtFlashColor = new Color(0.6f, 0.05f, 0.05f);

        private MazeLevel _level;
        private GridPlayerController _player;
        private PlayerTorch _torch;
        private MarkerPlacer _markers;
        private EchoCaster _echo;
        private TensionDirector _tensionDirector;
        private CrystalWallet _wallet;
        private LevelRewardCalculator _rewardCalculator;
        private AudioSource _audio;

        private Color _flashColor;
        private float _flashAlpha;

        public LevelOutcome Outcome { get; private set; } = LevelOutcome.InProgress;
        public int LastReward { get; private set; }

        public void Initialize(
            MazeLevel level,
            GridPlayerController player,
            PlayerTorch torch,
            MarkerPlacer markers,
            EchoCaster echo,
            TensionDirector tensionDirector,
            CrystalWallet wallet)
        {
            _level = level;
            _player = player;
            _torch = torch;
            _markers = markers;
            _echo = echo;
            _tensionDirector = tensionDirector;
            _wallet = wallet;
            _rewardCalculator = new LevelRewardCalculator();
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_flashAlpha > 0f)
                _flashAlpha = Mathf.Max(0f, _flashAlpha - Time.deltaTime * 0.8f);

            if (Outcome != LevelOutcome.InProgress) return;

            if (_tensionDirector != null && _tensionDirector.State == TensionState.Caught)
            {
                HandleCaught();
                return;
            }

            if (_player.CurrentCell.Equals(_level.Exit))
                HandleWin();
        }

        private void HandleCaught()
        {
            Outcome = LevelOutcome.Caught;
            TriggerFlash(CaughtFlashColor);
            ParticleEffects.SpawnBurst(_player.transform.position, new Color(0.5f, 0.1f, 0.1f), count: 30, speed: 4f, lifetime: 0.5f);
            Debug.Log("Yakalandın — tekrar denemek için Space'e bas.");
        }

        private void HandleWin()
        {
            Outcome = LevelOutcome.Won;

            double markerFraction = FractionOf(_markers.Economy.Stock, _markers.Economy.MaxStock);
            double torchFraction = FractionOf(_torch.Economy.Fuel, _torch.Economy.MaxFuel);
            double echoFraction = FractionOf(_echo.Economy.Charges, _echo.Economy.MaxCharges);

            LastReward = _rewardCalculator.CalculateReward(markerFraction, torchFraction, echoFraction);
            _wallet.Add(LastReward);
            MetaProgressionStore.SaveWallet(_wallet);
            _audio.PlayOneShot(SfxLibrary.Win);
            TriggerFlash(WinFlashColor);
            ParticleEffects.SpawnBurst(_player.transform.position, new Color(0.9f, 0.8f, 0.3f), count: 40, speed: 4.5f, lifetime: 0.7f);

            Debug.Log($"Kazandın! +{LastReward} Kristal (toplam {_wallet.Balance}). Adım: {_player.StepCount}. Space'e bas: yeni level.");

            // Lower step count is better, so the leaderboards must be configured "low to high" in App Store Connect.
            var gameCenter = GameCenterManager.Instance;
            if (gameCenter == null) return;

            gameCenter.SubmitScore(_player.StepCount, GameCenterIds.DailyLeaderboard);
            gameCenter.SubmitScore(_player.StepCount, GameCenterIds.MonthlyLeaderboard);

            if (_level.Patrol != null)
                gameCenter.ReportAchievement(GameCenterIds.FirstGuardEvadedAchievement);
        }

        private void TriggerFlash(Color color)
        {
            _flashColor = color;
            _flashAlpha = 0.6f;
        }

        private void OnGUI()
        {
            if (_flashAlpha <= 0f) return;

            var previous = GUI.color;
            GUI.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, _flashAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static double FractionOf(int value, int max) => max > 0 ? (double)value / max : 1.0;
    }
}
