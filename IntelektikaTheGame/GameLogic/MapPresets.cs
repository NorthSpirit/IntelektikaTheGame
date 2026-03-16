using IntelektikaTheGame.World;
using System;

namespace IntelektikaTheGame.GameLogic
{
    internal class MapPresets
    {
        public static void GenerateChokePointMap(GameWorld world)
        {
            // 1. Initialize EVERYTHING with Grass first using the Preset
            for (int x = 0; x < world.WorldWidth; x++)
            {
                for (int y = 0; y < world.WorldHeight; y++)
                {
                    world.Grid[x, y] = TilePresets.CreateTile(TileType.Grass);
                }
            }

            // 2. Add Borders and Central Wall
            for (int x = 0; x < world.WorldWidth; x++)
            {
                for (int y = 0; y < world.WorldHeight; y++)
                {
                    // Borders
                    if (x == 0 || x == world.WorldWidth - 1 || y == 0 || y == world.WorldHeight - 1)
                    {
                        world.Grid[x, y] = TilePresets.CreateTile(TileType.Mountain);
                    }

                    // Central Vertical Mountain Divider (Chokepoints at Y=7 and Y=22)
                    if (x == world.WorldWidth / 2)
                    {
                        if (y != 7 && y != 22)
                        {
                            world.Grid[x, y] = TilePresets.CreateTile(TileType.Mountain);
                            world.Grid[x, y].TileName = "Great Wall"; // Keeping your specific name override
                        }
                    }
                }
            }

            // 3. Generate Lakes
            GenerateLake(world, 10, 15, 3);
            GenerateLake(world, 40, 15, 3);

            // 4. Scatter Forests
            Random rng = new Random();
            for (int i = 0; i < 80; i++)
            {
                int rx = rng.Next(1, world.WorldWidth - 1);
                int ry = rng.Next(1, world.WorldHeight - 1);

                if (world.Grid[rx, ry].Type == TileType.Grass)
                {
                    world.Grid[rx, ry] = TilePresets.CreateTile(TileType.Forest);
                }
            }
        }

        private static void GenerateLake(GameWorld world, int centerX, int centerY, int radius)
        {
            int rSquared = radius * radius;
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (x < 0 || x >= world.WorldWidth || y < 0 || y >= world.WorldHeight) continue;

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if ((dx * dx) + (dy * dy) <= rSquared)
                    {
                        world.Grid[x, y] = TilePresets.CreateTile(TileType.Water);
                    }
                }
            }
        }
    }
}