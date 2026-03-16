using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelektikaTheGame.GameLogic;

namespace IntelektikaTheGame.World
{
    internal class Tile
    {
        public string TileName { get; set; }
        public GameLogic.TileType Type { get; set; }
        public string ArtUsed { get; set; }
        public Figurine? OccupyingUnit { get; set; }
    }
}
