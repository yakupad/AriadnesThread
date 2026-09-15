using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Meta;
using AriadnesThread.Core.Tension;
using AriadnesThread.GameCenterIntegration;
using AriadnesThread.Meta;
using AriadnesThread.Player;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// Watches for win (reaching the exit) or loss (Caught), computes the Kristal reward,
    /// persists it, and submits the result to Game Center. Submission never blocks anything
    /// else — see GameCenterManager's fail-soft design.
    /// </summary>
    public class LevelEndController : MonoBehaviour
    {
        private MazeLevel _level;
        private GridPlayerController _player;
        private PlayerTorch _torch;
        private MarkerPlacer _markers;
        private EchoCaster _echo;
        private TensionDirector _tensionDirector;
        private CrystalWallet _wallet;
        private LevelRewardCalculator _rewardCalculator;

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
        }

        private void Update()
        {
            if (Outcome != LevelOutcome.InProgress) return;

            if (_tensionDirector != null && _tensionDirector.State == TensionState.Caught)
            {
                Outcome = LevelOutcome.Caught;
                Debug.Log("Yakalandın — tekrar denemek için Space'e bas.");
                return;
            }

            if (_player.CurrentCell.Equals(_level.Exit))
                HandleWin();
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

            Debug.Log($"Kazandın! +{LastReward} Kristal (toplam {_wallet.Balance}). Adım: {_player.StepCount}. Space'e bas: yeni level.");

            // Lower step count is better, so the leaderboards must be configured "low to high" in App Store Connect.
            var gameCenter = GameCenterManager.Instance;
            if (gameCenter == null) return;

            gameCenter.SubmitScore(_player.StepCount, GameCenterIds.DailyLeaderboard);
            gameCenter.SubmitScore(_player.StepCount, GameCenterIds.MonthlyLeaderboard);

            if (_level.Patrol != null)
                gameCenter.ReportAchievement(GameCenterIds.FirstGuardEvadedAchievement);
        }

        private static double FractionOf(int value, int max) => max > 0 ? (double)value / max : 1.0;
    }
}
