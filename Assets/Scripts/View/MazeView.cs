using System.Collections.Generic;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>
    /// Builds simple cube geometry for a generated maze. Floor and wall cubes carry the
    /// default primitive BoxCollider, which the guard's vision-cone raycast (M3) uses for
    /// wall occlusion — no separate collision setup needed.
    /// </summary>
    public class MazeView : MonoBehaviour
    {
        public static readonly Color FloorColor = new Color(0.55f, 0.5f, 0.45f);
        private static readonly Color WallColor = new Color(0.32f, 0.29f, 0.27f);
        private static readonly Color StartColor = new Color(0.3f, 0.75f, 0.4f);
        private static readonly Color ExitColor = new Color(0.8f, 0.3f, 0.3f);
        private static readonly Color KeyRoomColor = new Color(0.85f, 0.8f, 0.2f);
        private static readonly Color LockedDoorColor = new Color(0.85f, 0.5f, 0.15f);

        /// <summary>Floor cube for each cell, keyed by coordinate — other systems (echo reveal) tint these.</summary>
        public Dictionary<CellCoord, GameObject> FloorTiles { get; } = new Dictionary<CellCoord, GameObject>();

        /// <summary>Set only when the level includes a timed gate — TimedGateView owns its per-frame color.</summary>
        public GameObject TimedGateMarker { get; private set; }

        public static Vector3 CellToWorld(CellCoord cell, float cellSize) =>
            new Vector3(cell.X * cellSize, 0f, cell.Y * cellSize);

        public void Build(MazeLevel level, float cellSize)
        {
            var grid = level.Grid;

            foreach (var cell in grid.AllCells())
            {
                BuildFloor(cell, cellSize);

                foreach (var dir in new[] { Direction.North, Direction.East })
                {
                    if (!grid.IsOpen(cell, dir))
                        BuildWallSegment(cell, dir, cellSize);
                }
            }

            BuildBoundaryWalls(grid, cellSize);
            MarkCell(level.Start, cellSize, StartColor, "Start");
            MarkCell(level.Exit, cellSize, ExitColor, "Exit");

            if (level.KeyLock != null)
            {
                MarkCell(level.KeyLock.KeyRoom, cellSize, KeyRoomColor, "KeyRoom");
                BuildDoorMarker(level.KeyLock.LockedDoorFrom, level.KeyLock.LockedDoorTo, cellSize, LockedDoorColor, "LockedDoor");
            }

            if (level.TimedGate != null)
                TimedGateMarker = BuildDoorMarker(level.TimedGate.From, level.TimedGate.To, cellSize, Color.white, "TimedGate");
        }

        private void BuildFloor(CellCoord cell, float cellSize)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"Floor_{cell}";
            floor.transform.SetParent(transform);
            floor.transform.position = CellToWorld(cell, cellSize) + new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(cellSize, 1f, cellSize);
            Colorize(floor, FloorColor);
            FloorTiles[cell] = floor;
        }

        private void BuildWallSegment(CellCoord cell, Direction dir, float cellSize)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{cell}_{dir}";
            wall.transform.SetParent(transform);

            var basePos = CellToWorld(cell, cellSize);
            var (offset, scale) = dir switch
            {
                Direction.North => (new Vector3(0, 0, cellSize * 0.5f), new Vector3(cellSize, 2f, 0.1f)),
                Direction.East => (new Vector3(cellSize * 0.5f, 0, 0), new Vector3(0.1f, 2f, cellSize)),
                _ => throw new System.ArgumentOutOfRangeException(nameof(dir), "Only North/East build a wall segment; South/West are the neighbor's.")
            };

            wall.transform.position = basePos + offset + Vector3.up;
            wall.transform.localScale = scale;
            Colorize(wall, WallColor);
        }

        /// <summary>The outermost ring has no neighbor on the far side, so North/East-only
        /// wall building above misses the grid's South and West boundary — close it here.</summary>
        private void BuildBoundaryWalls(MazeGrid grid, float cellSize)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                var bottom = new CellCoord(x, 0);
                if (!grid.IsOpen(bottom, Direction.South))
                    BuildWallSegment(new CellCoord(x, -1), Direction.North, cellSize);
            }

            for (int y = 0; y < grid.Height; y++)
            {
                var left = new CellCoord(0, y);
                if (!grid.IsOpen(left, Direction.West))
                    BuildWallSegment(new CellCoord(-1, y), Direction.East, cellSize);
            }
        }

        private void MarkCell(CellCoord cell, float cellSize, Color color, string label)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"Marker_{label}_{cell}";
            marker.transform.SetParent(transform);
            marker.transform.position = CellToWorld(cell, cellSize) + Vector3.up * 0.05f;
            marker.transform.localScale = new Vector3(cellSize * 0.5f, 0.1f, cellSize * 0.5f);
            Colorize(marker, color);
        }

        /// <summary>A thin overlay straddling an open edge — visual signaling only,
        /// gameplay-blocking (if any) is enforced by the player's own movement logic, not a collider.</summary>
        private GameObject BuildDoorMarker(CellCoord from, CellCoord to, float cellSize, Color color, string label)
        {
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = $"Door_{label}_{from}_{to}";
            door.transform.SetParent(transform);
            Destroy(door.GetComponent<Collider>()); // never physically blocks — see class doc

            var midpoint = Vector3.Lerp(CellToWorld(from, cellSize), CellToWorld(to, cellSize), 0.5f) + Vector3.up;
            door.transform.position = midpoint;

            bool alongX = from.Y == to.Y;
            door.transform.localScale = alongX ? new Vector3(0.15f, 1.8f, cellSize * 0.8f) : new Vector3(cellSize * 0.8f, 1.8f, 0.15f);

            Colorize(door, color);
            return door;
        }

        private static void Colorize(GameObject go, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            go.GetComponent<Renderer>().material = material;
        }
    }
}
