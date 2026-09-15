using AriadnesThread.CameraControl;
using AriadnesThread.Core.Generation;
using AriadnesThread.Guard;
using AriadnesThread.Player;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// M3 scope: adds the guard, torch, and tension director on top of M2's generation +
    /// render + tap-to-move. Key/lock is still off (that's M4). The on-screen HUD and the
    /// console log in TensionDirector exist purely to feel the state machine working —
    /// there is no real level-fail/retry flow yet (M4/M5).
    /// </summary>
    public class MazeBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 1;
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 12;
        [SerializeField] private float cellSize = 3f;

        private GridPlayerController _player;
        private PlayerTorch _torch;
        private TensionDirector _tensionDirector;

        private void Start()
        {
            var parameters = new LevelParameters
            {
                Width = width,
                Height = height,
                IncludeGuard = true,
                IncludeKeyLock = false,
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
                        $"Meşale: {(_torch.Economy.IsLit ? "açık" : "kapalı")} ({_torch.Economy.Fuel}/{_torch.Economy.MaxFuel}) — T'ye bas\n" +
                        (_tensionDirector != null ? $"Durum: {_tensionDirector.State}" : "Durum: gardiyan yok");

            GUI.Label(new Rect(10, 10, 400, 80), lines);
        }
    }
}
