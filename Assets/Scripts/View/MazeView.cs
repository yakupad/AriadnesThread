using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>
    /// Builds simple cube geometry for a generated maze. Floor and wall cubes carry the
    /// default primitive BoxCollider, which the guard's vision-cone raycast (M3) will use
    /// for wall occlusion — no separate collision setup needed later.
    /// </summary>
    public class MazeView : MonoBehaviour
    {
        private static readonly Color FloorColor = new Color(0.55f, 0.5f, 0.45f);
        private static readonly Color WallColor = new Color(0.32f, 0.29f, 0.27f);
        private static readonly Color StartColor = new Color(0.3f, 0.75f, 0.4f);
        private static readonly Color ExitColor = new Color(0.8f, 0.3f, 0.3f);

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
        }

        private void BuildFloor(CellCoord cell, float cellSize)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"Floor_{cell}";
            floor.transform.SetParent(transform);
            floor.transform.position = CellToWorld(cell, cellSize) + new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(cellSize, 1f, cellSize);
            Colorize(floor, FloorColor);
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

        private static void Colorize(GameObject go, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            go.GetComponent<Renderer>().material = material;
        }
    }
}
