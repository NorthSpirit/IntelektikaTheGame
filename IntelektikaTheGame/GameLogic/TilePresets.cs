using IntelektikaTheGame.World;

namespace IntelektikaTheGame.GameLogic
{
    internal class TilePresets
    {
        public static Tile CreateTile(TileType type)
        {
            return type switch
            {
                TileType.Grass => new Tile
                {
                    Type = TileType.Grass,
                    TileName = "Plains",
                    ArtUsed = "tile_grass"
                },
                TileType.Forest => new Tile
                {
                    Type = TileType.Forest,
                    TileName = "Woods",
                    ArtUsed = "tile_forest"
                },
                TileType.Water => new Tile
                {
                    Type = TileType.Water,
                    TileName = "Lake",
                    ArtUsed = "tile_water"
                },
                TileType.Mountain => new Tile
                {
                    Type = TileType.Mountain,
                    TileName = "Peak",
                    ArtUsed = "tile_mountain"
                },
                _ => new Tile { Type = TileType.Grass, TileName = "Unknown", ArtUsed = "tile_grass" }
            };
        }
    }
}