using AriadnesThread.CameraControl;
using AriadnesThread.Core.Generation;
using AriadnesThread.Player;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// M2 scope: generation + isometric render + tap-to-move only. Guard and key/lock are
    /// deliberately off here (that's M3/M4) — there is no zone data yet for a "stop at the
    /// tension-zone boundary" hook to act on, so that part of the M2 plan is a no-op until
    /// a guard actually exists.
    /// </summary>
    public class MazeBootstrap : MonoBehaviour
    {
        [SerializeField] private int seed = 1;
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 12;
        [SerializeField] private float cellSize = 3f;

        private void Start()
        {
            var parameters = new LevelParameters
            {
                Width = width,
                Height = height,
                IncludeGuard = false,
                IncludeKeyLock = false,
            };

            var level = MazePipeline.Generate(seed, parameters);

            var viewGO = new GameObject("MazeView");
            var view = viewGO.AddComponent<MazeView>();
            view.Build(level, cellSize);

            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Player";
            playerGO.transform.position = MazeView.CellToWorld(level.Start, cellSize) + Vector3.up;
            var player = playerGO.AddComponent<GridPlayerController>();
            player.Initialize(level, cellSize);

            var lightGO = new GameObject("Sun");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camGO = new GameObject("IsometricCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(width, height) * cellSize * 0.6f;
            camGO.tag = "MainCamera";
            var follow = camGO.AddComponent<IsometricCameraFollow>();
            follow.SetTarget(playerGO.transform);
        }
    }
}
