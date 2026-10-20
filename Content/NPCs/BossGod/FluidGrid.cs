using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace Calamitytwinklefragment.Content.NPCs.BossGod
{
    public class FluidGrid(int cellSize)
    {
        private readonly Dictionary<Point, List<int>> grid = [];
        private readonly int cellSize = cellSize;

        public void Clear()
        {
            grid.Clear();
        }

        public void Add(Vector2 position, int npcIndex)
        {
            Point cell = GetCell(position);
            if (!grid.TryGetValue(cell, out List<int> value))
            {
                value = [];
                grid[cell] = value;
            }

            value.Add(npcIndex);
        }

        public List<int> GetNearby(Vector2 position, float radius)
        {
            List<int> result = [];
            float radiusSquared = radius * radius;

            Point centerCell = GetCell(position);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Point checkCell = new(centerCell.X + x, centerCell.Y + y);
                    if (grid.TryGetValue(checkCell, out List<int> cellContents))
                    {
                        foreach (int npcIndex in cellContents)
                        {
                            if (npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
                            {
                                float distanceSquared = Vector2.DistanceSquared(position, Main.npc[npcIndex].Center);
                                if (distanceSquared <= radiusSquared)
                                {
                                    result.Add(npcIndex);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private Point GetCell(Vector2 position)
        {
            return new Point((int)(position.X / cellSize), (int)(position.Y / cellSize));
        }
    }
}