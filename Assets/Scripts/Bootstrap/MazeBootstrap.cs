using AriadnesThread.CameraControl;
using AriadnesThread.Core.Generation;
using AriadnesThread.Guard;
using AriadnesThread.Player;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// M4 scope: adds markers, echo, key/lock, and a timed gate on top of M3's guard/torch/
    /// tension loop. The HUD and console logging are still prototype-only diagnostics, not a
    /// real UI (that's M5).
    /// </summary>
    public class MazeBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 1;
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 12;
        [SerializeField] private float cellSize = 3f;

        private GridPlayerController _player;
        private PlayerTorch _torch;
        private MarkerPlacer _markers;
        private EchoCaster _echo;
        private TensionDirector _tensionDirector;

        private void Start()
        {
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
            var view = viewGO.AddComponent<MazeView>();
            view.Build(level, cellSize);

            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Player";
            playerGO.transform.position = MazeView.CellToWorld(level.Start, cellSize) + Vector3.up;
            _player = playerGO.AddComponent<GridPlayerController>();
            _player.Initialize(level, cellSize);

            _torch = playerGO.AddComponent<PlayerTorch>();
            _markers = playerGO.AddComponent<MarkerPlacer>();
            _markers.Initialize(level);
            _echo = playerGO.AddComponent<EchoCaster>();
            _echo.Initialize(level);

            var markerViewGO = new GameObject("MarkerView");
            markerViewGO.AddComponent<MarkerView>().Initialize(_markers.Economy, cellSize);

            var echoViewGO = new GameObject("EchoView");
            echoViewGO.AddComponent<EchoView>().Initialize(_echo.Economy, view);

            if (level.TimedGate != null)
            {
                _player.OnStepTaken += level.TimedGate.OnStep;
                var gateViewGO = new GameObject("TimedGateView");
                gateViewGO.AddComponent<TimedGateView>().Initialize(level.TimedGate, view.TimedGateMarker);
            }

            var lightGO = new GameObject("Sun");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.5f; // dim — the torch's point light should matter
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camGO = new GameObject("IsometricCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(width, height) * cellSize * 0.6f;
            camGO.tag = "MainCamera";
            var follow = camGO.AddComponent<IsometricCameraFollow>();
            follow.SetTarget(playerGO.transform);

            GuardController guard = null;
            if (level.Patrol != null)
            {
                var guardGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                guardGO.name = "Guard";
                guardGO.GetComponent<Renderer>().material.color = new Color(0.7f, 0.15f, 0.15f);
                guard = guardGO.AddComponent<GuardController>();
                guard.Initialize(level, cellSize);
            }

            if (guard != null)
            {
                var directorGO = new GameObject("TensionDirector");
                _tensionDirector = directorGO.AddComponent<TensionDirector>();
                _tensionDirector.Initialize(level, _player, guard);
            }
            else
            {
                Debug.LogWarning("No patrol loop could be generated for this seed/size — playing without a guard.");
            }
        }

        private void OnGUI()
        {
            if (_player == null) return;

            var lines = $"Adım: {_player.StepCount}\n" +
                        $"Meşale: {(_torch.Economy.IsLit ? "açık" : "kapalı")} ({_torch.Economy.Fuel}/{_torch.Economy.MaxFuel}) — T\n" +
                        $"İşaret: {_markers.Economy.Stock} — G koy, Shift+G ters renk, R topla\n" +
                        $"Yankı: {_echo.Economy.Charges} — E\n" +
                        $"Anahtar: {(_player.HasKey ? "aldın" : "yok")}\n" +
                        (_tensionDirector != null ? $"Durum: {_tensionDirector.State}" : "Durum: gardiyan yok");

            GUI.Label(new Rect(10, 10, 420, 130), lines);
        }
    }
}
