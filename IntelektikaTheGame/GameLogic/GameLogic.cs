using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IntelektikaTheGame.GameLogic
{
    //Tile types for variation
    public enum TileType
    {
        Grass,
        Forest,
        Water,
        Mountain
    }

    internal class PathNode
    {
        //Position of the node in the world 
        public int XCord { get; set; }
        public int YCord { get; set; }
        //Cost From the Start 
        public double CFS { get; set; }
        //Heuristic distance to the end. (Distance To Goal)
        public double DTG { get; set; }
        //Total Cost
        public double TC => CFS + DTG;
        //Reference of the previous node in the path
        public PathNode Parent { get; set; }

        public PathNode(int x, int y)
        {
            XCord = x; YCord = y;
        }
    }

    internal class GameLogic
    {
        public enum UnitType
        {
            Melee,
            Archer,
            Flier,
            Healer
        }

        public enum player
        {
            Attacker,
            Defender
        }

        //Calculates the cost for figurine to travel 
        private double GetMovementCost(TileType type, UnitType unit)
        {
            //Movement Rules:
            //Grass = 1 to 1.
            //Forest is 25% penalty, Mountain and Water is impassable
            //For fliers, everything is passable, but mountain adds penalty.
            //Flies
            if (unit == UnitType.Flier)
            {
                if (type == TileType.Mountain) return 1.5;
                return 1.0; //Fliers ignore water/forest penalties
            }

            // Non-fliers
            return type switch
            {
                TileType.Grass => 1.0,
                TileType.Forest => 1.25,
                _ => double.PositiveInfinity //Water and Mountains are untraversable
            };
        }
        public List<World.Tile> GetPath(World.GameWorld world, Figurine unit, int targetX, int targetY, player currentTeam)
        {
            //1. Get Start Coords
            (int startX, int startY) = GetUnitCoords(unit, world);
            if (startX == -1 || (startX == targetX && startY == targetY)) return new List<World.Tile>();

            var openList = new List<PathNode>();
            var closedList = new HashSet<string>();

            var startNode = new PathNode(startX, startY) { CFS = 0, DTG = GetManhattanDist(startX, startY, targetX, targetY) };
            openList.Add(startNode);

            PathNode bestNodeSoFar = startNode;

            while (openList.Count > 0)
            {
                //Get node with lowest Total Cost
                var currentNode = openList.OrderBy(n => n.TC).First();
                openList.Remove(currentNode);

                //Keep track of the node that got us closest to the goal (for partial moves)
                if (currentNode.DTG < bestNodeSoFar.DTG) bestNodeSoFar = currentNode;

                //Found the exact goal
                if (currentNode.XCord == targetX && currentNode.YCord == targetY)
                    return ReconstructPath(world, currentNode, unit.FigurineMovementPointsMax);

                closedList.Add($"{currentNode.XCord},{currentNode.YCord}");

                foreach (var (nx, ny) in GetNeighbors(currentNode.XCord, currentNode.YCord, world))
                {
                    if (closedList.Contains($"{nx},{ny}")) continue;

                    var targetTile = world.Grid[nx, ny];

                    //Enemies block movement, allies do NOT block movement (you can walk through them)
                    //but you cannot END your turn on an ally (handled in MoveUnit).
                    if (targetTile.OccupyingUnit != null && targetTile.OccupyingUnit.Owner != currentTeam) continue;

                    double moveCost = GetMovementCost(targetTile.Type, unit.UnitType);
                    if (double.IsInfinity(moveCost)) continue;

                    double newCFS = currentNode.CFS + moveCost;


                    var neighbor = openList.FirstOrDefault(n => n.XCord == nx && n.YCord == ny);
                    if (neighbor == null)
                    {
                        neighbor = new PathNode(nx, ny)
                        {
                            Parent = currentNode,
                            CFS = newCFS,
                            DTG = GetManhattanDist(nx, ny, targetX, targetY)
                        };
                        openList.Add(neighbor);
                    }
                    else if (newCFS < neighbor.CFS)
                    {
                        neighbor.CFS = newCFS;
                        neighbor.Parent = currentNode;
                    }
                }
            }

            //If the path couldn't be reached one close enough is returned instead.
            return ReconstructPath(world, bestNodeSoFar, unit.FigurineMovementPointsMax);
        }

        private List<World.Tile> ReconstructPath(World.GameWorld world, PathNode node, int maxMove)
        {
            var path = new List<World.Tile>();
            while (node != null)
            {
                //Only add tiles that the unit can actually afford to reach this turn
                if (node.CFS <= maxMove)
                {
                    path.Add(world.Grid[node.XCord, node.YCord]);
                }
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        //In a houlistic square grid, corner neighbours are not checked
        private int GetManhattanDist(int x1, int y1, int x2, int y2) => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

        private IEnumerable<(int, int)> GetNeighbors(int x, int y, World.GameWorld world)
        {
            if (x > 0) yield return (x - 1, y);
            if (x < world.WorldWidth - 1) yield return (x + 1, y);
            if (y > 0) yield return (x, y - 1);
            if (y < world.WorldHeight - 1) yield return (x, y + 1);
        }

        private (int, int) GetUnitCoords(Figurine unit, World.GameWorld world)
        {
            for (int x = 0; x < world.WorldWidth; x++)
            {
                for (int y = 0; y < world.WorldHeight; y++)
                {
                    if (world.Grid[x, y].OccupyingUnit == unit)
                    {
                        return (x, y);
                    }
                }
            }
            //Unit not found on grid, should be impossible...
            return (-1, -1);
        }
    }
}
