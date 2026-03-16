using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelektikaTheGame.World
{
    internal class GameWorld
    {
        public int WorldWidth { get; set; }
        public int WorldHeight { get; set; }
        public Tile[,] Grid { get; }

        public GameWorld(int width, int height)
        {
            WorldWidth = width;
            WorldHeight = height;
            Grid = new Tile[width, height];
        }
    }
}
