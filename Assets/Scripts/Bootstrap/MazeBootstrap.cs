using AriadnesThread.CameraControl;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Meta;
using AriadnesThread.GameCenterIntegration;
using AriadnesThread.Guard;
using AriadnesThread.Meta;
using AriadnesThread.Player;
using AriadnesThread.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// M5 scope + a visual/audio polish pass: Kristal/meta-progression, Game Center, a real
    /// (if minimal) level-end flow, dark ambient lighting so the torch mechanic actually
    /// matters, and TensionAtmosphere tying camera/lighting/audio to the tension state.
    /// Retry regenerates the SAME seed with run-time resources reset — no checkpoint — per
    /// the concept doc's retry rules.
    /// </summary>
    public class MazeBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 1;
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 12;
        [SerializeField] private float cellSize = 3f;

        private Transform _levelRoot;
        private CrystalWallet _wallet;
        private GameCenterManager _gameCenter;

        private GridPlayerController _player;
        private PlayerTorch _torch;
        private MarkerPlacer _markers;
        private EchoCaster _echo;
        private TensionDirector _tensionDirector;
        private LevelEndController _levelEnd;

        private void Awake()
        {
            _wallet = MetaProgressionStore.LoadWallet();

            var gameCenterGO = new GameObject("GameCenter");
            gameCenterGO.transform.SetParent(transform);
            _gameCenter = gameCenterGO.AddComponent<GameCenterManager>();
            _gameCenter.Authenticate();

            // Calm baseline — TensionAtmosphere only ever lerps away from here, so start there.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.03f, 0.03f, 0.05f);
            RenderSettings.fogDensity = 0.035f;
        }

        private void Start() => BuildLevel();

        private void Update()
        {
            if (_levelEnd != null && _levelEnd.Outcome != LevelOutcome.InProgress
                && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                RetryLevel();
        }

        private void RetryLevel()
        {
            Destroy(_levelRoot.gameObject);
            BuildLevel(); // same seed — see class doc
        }

        private void BuildLevel()
        {
            _levelRoot = new GameObject("LevelRoot").transform;

            var parameters = new LevelParameters
            {
                Width = width,
                Height = height,
                IncludeGuard = true,
                IncludeKeyLock = true,
                IncludeTimedGate = true,
            };

            var level = MazePipeline.Generate(seed, parameters);

            var viewGO = new GameObject("MazeView");
            viewGO.transform.SetParent(_levelRoot);
            var view = viewGO.AddComponent<MazeView>();
            view.Build(level, cellSize);

            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Player";
            playerGO.transform.SetParent(_levelRoot);
            playerGO.transform.position = MazeView.CellToWorld(level.Start, cellSize) + Vector3.up;
            _player = playerGO.AddComponent<GridPlayerController>();
            _player.Initialize(level, cellSize);

            _torch = playerGO.AddComponent<PlayerTorch>();
            _markers = playerGO.AddComponent<MarkerPlacer>();
            _markers.Initialize(level);
            _echo = playerGO.AddComponent<EchoCaster>();
            _echo.Initialize(level);
            playerGO.AddComponent<PlayerAudio>();

            var markerViewGO = new GameObject("MarkerView");
            markerViewGO.transform.SetParent(_levelRoot);
            markerViewGO.AddComponent<MarkerView>().Initialize(_markers.Economy, cellSize);

            var echoViewGO = new GameObject("EchoView");
            echoViewGO.transform.SetParent(_levelRoot);
            echoViewGO.AddComponent<EchoView>().Initialize(_echo.Economy, view);

            if (level.TimedGate != null)
            {
                _player.OnStepTaken += level.TimedGate.OnStep;
                var gateViewGO = new GameObject("TimedGateView");
                gateViewGO.transform.SetParent(_levelRoot);
                gateViewGO.AddComponent<TimedGateView>().Initialize(level.TimedGate, view.TimedGateMarker);
            }

            var lightGO = new GameObject("Sun");
            lightGO.transform.SetParent(_levelRoot);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.6f, 0.65f, 0.8f); // cool moonlight — the torch's warm glow should read as contrast
            light.intensity = 0.15f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camGO = new GameObject("IsometricCamera");
            camGO.transform.SetParent(_levelRoot);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(width, height) * cellSize * 0.6f;
            cam.clearFlags = CameraClearFlags.SolidColor; // no skybox — it ignored the dark ambient/fog tuning
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
            camGO.tag = "MainCamera";
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<IsometricCameraFollow>().SetTarget(playerGO.transform);

            GuardController guard = null;
            if (level.Patrol != null)
            {
                var guardGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                guardGO.name = "Guard";
                guardGO.transform.SetParent(_levelRoot);
                guardGO.GetComponent<Renderer>().material.color = new Color(0.7f, 0.15f, 0.15f);
                guard = guardGO.AddComponent<GuardController>();
                guard.Initialize(level, cellSize);
            }
            else
            {
                Debug.LogWarning("No patrol loop could be generated for this seed/size — playing without a guard.");
            }

            _tensionDirector = null;
            if (guard != null)
            {
                var directorGO = new GameObject("TensionDirector");
                directorGO.transform.SetParent(_levelRoot);
                _tensionDirector = directorGO.AddComponent<TensionDirector>();
                _tensionDirector.Initialize(level, _player, guard);

                var atmosphereGO = new GameObject("TensionAtmosphere");
                atmosphereGO.transform.SetParent(_levelRoot);
                var sfxSource = atmosphereGO.AddComponent<AudioSource>();
                var droneSource = atmosphereGO.AddComponent<AudioSource>();
                var atmosphere = atmosphereGO.AddComponent<TensionAtmosphere>();
                atmosphere.Initialize(_tensionDirector, cam, guard.transform, sfxSource, droneSource);
            }

            var levelEndGO = new GameObject("LevelEndController");
            levelEndGO.transform.SetParent(_levelRoot);
            _levelEnd = levelEndGO.AddComponent<LevelEndController>(); // [RequireComponent] adds its AudioSource
            _levelEnd.Initialize(level, _player, _torch, _markers, _echo, _tensionDirector, _wallet);
        }

        private void OnGUI()
        {
            if (_player == null) return;

            TouchUIGuard.ClearFrame();

            // Default IMGUI font reads as tiny on a real phone's pixel density — scale it
            // against screen width instead of hardcoding a point size tuned for the Simulator.
            int fontSize = Mathf.RoundToInt(Screen.width / 45f);
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;

            DrawStatusLabel();
            DrawActionButtons();

            if (_levelEnd != null && _levelEnd.Outcome != LevelOutcome.InProgress)
                DrawRetryButton();
        }

        private void DrawStatusLabel()
        {
            var lines = $"Kristal: {_wallet.Balance}\n" +
                        $"Adım: {_player.StepCount}\n" +
                        $"Meşale: {(_torch.Economy.IsLit ? "açık" : "kapalı")} ({_torch.Economy.Fuel}/{_torch.Economy.MaxFuel})\n" +
                        $"İşaret: {_markers.Economy.Stock}\n" +
                        $"Yankı: {_echo.Economy.Charges}\n" +
                        $"Anahtar: {(_player.HasKey ? "aldın" : "yok")}\n" +
                        (_tensionDirector != null ? $"Durum: {_tensionDirector.State}" : "Durum: gardiyan yok");

            GUI.Label(new Rect(10, 10, Screen.width * 0.6f, Screen.height * 0.25f), lines);
        }

        // Touch stand-ins for the T/G/Shift+G/R/E keyboard shortcuts — a phone has no
        // keyboard, so without these the marker/torch/echo systems are unreachable on device.
        private void DrawActionButtons()
        {
            float buttonHeight = Screen.height * 0.09f;
            float buttonWidth = Screen.width / 4f;
            float y = Screen.height - buttonHeight - 20f;

            var torchRect = new Rect(0, y, buttonWidth, buttonHeight);
            TouchUIGuard.ClaimRect(torchRect);
            if (GUI.Button(torchRect, "Meşale"))
                _torch.ToggleTorch();

            var markerRect = new Rect(buttonWidth, y, buttonWidth, buttonHeight);
            TouchUIGuard.ClaimRect(markerRect);
            if (GUI.Button(markerRect, "İşaret"))
                _markers.PlaceOrRecolorAtCurrentCell(forceOpposite: false);

            var retrieveRect = new Rect(buttonWidth * 2, y, buttonWidth, buttonHeight);
            TouchUIGuard.ClaimRect(retrieveRect);
            if (GUI.Button(retrieveRect, "Topla"))
                _markers.RetrieveAtCurrentCell();

            var echoRect = new Rect(buttonWidth * 3, y, buttonWidth, buttonHeight);
            TouchUIGuard.ClaimRect(echoRect);
            if (GUI.Button(echoRect, "Yankı"))
                _echo.CastEcho();
        }

        private void DrawRetryButton()
        {
            float width = Screen.width * 0.5f;
            float height = Screen.height * 0.08f;
            var rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.55f, width, height);
            TouchUIGuard.ClaimRect(rect);

            if (GUI.Button(rect, $"{_levelEnd.Outcome} — Yeni Level"))
                RetryLevel();
        }
    }
}
