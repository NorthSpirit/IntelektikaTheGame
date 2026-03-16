using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelektikaTheGame.GameLogic
{
    //The primary class, responsible for the behaviour of each figurine (agent).
    //AIBrain is responsible for the decision making, movement and actions of each figurine, based on a small tree.
    //Which involves the type of AI, the AI's owner circumstances of the actor and it's environment.
    internal class AIBrain
    {
        //Main method of the AI, which works in 3 steps:
        //1st) Before taking an action, the actor tries to find the target.
        //2nd) Movement towards the target.
        //3rd) If actor didn't perform the action yet, it can perform it AFTER moving.
        public void ExecuteUnitTurn(Figurine unit, World.GameWorld world, GameLogic logic, GameLogic.player currentTeam)
        {
            //Based on figurine's Owner and UnitType, find the target.
            Figurine primaryTarget = FindBestTarget(unit, world, currentTeam);
            unit.CurrentVisualPath.Clear(); //Clear out the visualization indicators to declutter the map.
            bool hasAttacked = false;

            //1. If actor is already in range it performs Attack/Heal BEFORE moving.
            if (primaryTarget != null && IsInActionRange(unit, primaryTarget, world))
            {
                PerformAction(unit, primaryTarget, world);
                hasAttacked = true;

                //Set the path type for visual clarity.
                unit.PathType = unit.UnitType == GameLogic.UnitType.Healer ? "heal" : "attack";
            }

            //2. Movement Logic, calculates and executes based on the decision tree.
            MoveUnit(unit, world, logic, currentTeam, primaryTarget);

            //3. Attack/Heal AFTER moving if not already done (allows actor to get in range and then act)
            if (!hasAttacked)
            {
                primaryTarget = FindBestTarget(unit, world, currentTeam);
                if (primaryTarget != null && IsInActionRange(unit, primaryTarget, world))
                {
                    PerformAction(unit, primaryTarget, world);
                    //Set the path type for visual clarity.
                    unit.PathType = unit.UnitType == GameLogic.UnitType.Healer ? "heal" : "attack";
                }
            }
        }

        //A simple (Heuristic) targeting system, which avoids complicated action weights, instead using a decision tree, based on player, actor type and other factors.
        //Prioritizes the most vulnerable targets for agressive actors - healers and archers and after that, targets with lowest health points.
        //For healers prioritize targets with lowest HP
        //Defender player also prioritizes protecting it's own units by targeting enemies, which are within 10 tiles (magic number) away from his units.
        //Defender still prioritizes enemy fliers, since they prioritize defender's healers.
        //threats logic works for this small project, but it's not optimal.
        private Figurine FindBestTarget(Figurine unit, World.GameWorld world, GameLogic.player currentTeam)
        {
            var allUnits = GetAllUnits(world);
            var enemies = allUnits.Where(u => u.Owner != currentTeam).ToList();
            var allies = allUnits.Where(u => u.Owner == currentTeam).ToList();
            if (!enemies.Any()) return null;

            //Healer focuses on injured (not full HP) ally with the lowest health points.
            if (unit.UnitType == GameLogic.UnitType.Healer)
            {
                return allUnits.Where(u => u.Owner == currentTeam && u.FigurineHealthCurrent < u.FigurineHealthMax)
                               .OrderBy(u => u.FigurineHealthCurrent)
                               .FirstOrDefault();
            }

            //DEFENDER TARGETING LOGIC
            if (currentTeam == GameLogic.player.Defender)
            {
                //Search for threats - enemies, which are 10 tiles away from any of the defender's allies.
                var threats = enemies.Where(e =>
                {
                    (int ex, int ey) = GetUnitCoords(e, world);

                    //Pick one, which is the closest
                    return allies.Any(a =>
                    {
                        (int ax, int ay) = GetUnitCoords(a, world);
                        int dist = Math.Abs(ex - ax) + Math.Abs(ey - ay);
                        return dist <= 10;
                    });
                }).OrderByDescending(e => e.UnitType == GameLogic.UnitType.Flier).ThenByDescending(e => e.UnitType == GameLogic.UnitType.Healer) //Defender still tries to prioritize the fliers (since they are most "annoying", then the healers.
                  .ThenBy(e => e.FigurineHealthCurrent);

                if (threats.Any()) return threats.First();
            }

            //Attackers prioritize healers, then archers and then units with lowest HP, trying to poke down the most vulnerable units.
            return enemies.OrderByDescending(e => e.UnitType == GameLogic.UnitType.Healer)
                  .ThenByDescending(e => e.UnitType == GameLogic.UnitType.Archer)
                  .ThenBy(e => e.FigurineHealthCurrent)
                  .FirstOrDefault();
        }

        //Movement logic of the AIBrain
        //Uses the unit's type, player's team and other factor to determine the "goal" coordinates.
        //And then uses A STAR algorithm to reach it.
        private void MoveUnit(Figurine unit, World.GameWorld world, GameLogic logic, GameLogic.player currentTeam, Figurine primaryTarget)
        {
            (int startX, int startY) = GetUnitCoords(unit, world);
            int goalX = startX, goalY = startY;
            int chokeX = world.WorldWidth / 2;

            var allUnits = GetAllUnits(world);
            var myTeam = allUnits.Where(u => u.Owner == currentTeam).ToList();

            //Thread detection logic - checks if any enemies are threatening any of the allied units (matches the FindBestTarget logic)
            //Self note: in a real game, I should exclude fliers from this...
            Figurine nearestThreat = allUnits
                .Where(u => u.Owner != currentTeam)
                .OrderBy(e =>
                {
                    (int ex, int ey) = GetUnitCoords(e, world);
                    //Distance to the closest ally
                    return myTeam.Min(a =>
                    {
                        (int ax, int ay) = GetUnitCoords(a, world);
                        return Math.Abs(ex - ax) + Math.Abs(ey - ay);
                    });
                }).FirstOrDefault();

            bool isIntruderAlert = false;
            if (nearestThreat != null)
            {
                (int nx, int ny) = GetUnitCoords(nearestThreat, world);
                //Same as in FindBestTarget, distance is set to 10 tiles.
                int distToClosestAlly = myTeam.Min(a =>
                {
                    (int ax, int ay) = GetUnitCoords(a, world);
                    return Math.Abs(nx - ax) + Math.Abs(ny - ay);
                });

                if (distToClosestAlly <= 10) isIntruderAlert = true;
            }

            //DEFENDER LOGIC BRANCH (Based on positioning and reaction)
            if (currentTeam == GameLogic.player.Defender)
            {
                //If we have a primaryTarget from FindBestTarget, and we are in Intruder Alert, we should move relative to THAT target.
                Figurine activeTarget = (isIntruderAlert && primaryTarget != null) ? primaryTarget : nearestThreat;

                if (isIntruderAlert && activeTarget != null)
                {
                    (int tx, int ty) = GetUnitCoords(activeTarget, world);

                    if (unit.UnitType == GameLogic.UnitType.Archer)
                        (goalX, goalY) = FindOptimalRangeTile(world, startX, startY, tx, ty, 3);
                    else if (unit.UnitType == GameLogic.UnitType.Healer)
                    {
                        int avgTeamX = (int)myTeam.Average(u => GetUnitCoords(u, world).Item1); //Find the average location of the team
                        goalX = avgTeamX + 2; //Stay 2 tiles behind the "center of mass"
                        goalY = (int)myTeam.Average(u => GetUnitCoords(u, world).Item2);
                    }
                    else
                        (goalX, goalY) = (tx, ty); //Collapse on the target
                }
                else
                {
                    //Formation logic:
                    //Melee units try to get to the choke point, calculated by the helper method
                    //Archer tries to stand 2 tiles behind the front line.
                    //Healer tries to stand in the middle of the allied team.
                    //Fliers try to be behind melee
                    var myMelee = myTeam.Where(u => u.UnitType == GameLogic.UnitType.Melee).ToList();
                    int frontLineX = myMelee.Any() ? (int)myMelee.Average(u => GetUnitCoords(u, world).Item1) : chokeX;

                    switch (unit.UnitType)
                    {
                        case GameLogic.UnitType.Melee: (goalX, goalY) = FindChokePoint(world, unit, logic); break;
                        case GameLogic.UnitType.Archer: goalX = frontLineX + 2; goalY = startY; break;
                        case GameLogic.UnitType.Healer: goalX = frontLineX + 4; goalY = (int)myTeam.Average(u => GetUnitCoords(u, world).Item2); break;
                        case GameLogic.UnitType.Flier: goalX = frontLineX + 1; goalY = startY; break;
                    }
                }
            }
            //ATTACKER LOGIC (Proactive)
            else
            {
                //Attacker doesn't get intruder alerts and instead focuses strictly on primaryTarget
                if (primaryTarget != null)
                {
                    (int tx, int ty) = GetUnitCoords(primaryTarget, world);

                    if (unit.UnitType == GameLogic.UnitType.Flier) //Fliers try to mimic swooping behaviour by flanking - they will move the to the edge of the map and line themselves with the target, before attacking.
                    {
                        int edgeY = (startY < world.WorldHeight / 2) ? 0 : world.WorldHeight - 1;
                        goalX = tx;
                        goalY = (Math.Abs(startX - tx) > 3) ? edgeY : ty;
                    }
                    else if (unit.UnitType == GameLogic.UnitType.Healer)
                    {
                        // Healers target allies, FindBestTarget already returned the best wounded ally
                        (int tx_ally, int ty_ally) = GetUnitCoords(primaryTarget, world);
                        goalX = tx_ally + (startX < tx_ally ? -1 : 1);
                        goalY = ty_ally;
                    } //Archer always try to be in the perfect range (3) to attack, move in closer if too far, get closer if not close enough.
                    else if (unit.UnitType == GameLogic.UnitType.Archer)
                    {
                        (goalX, goalY) = FindOptimalRangeTile(world, startX, startY, tx, ty, 3);
                    }
                    else //Melee (and other future types) doesn't have special interactions
                    {
                        (goalX, goalY) = (tx, ty);
                    }
                }
                else if (unit.UnitType == GameLogic.UnitType.Healer && myTeam.Count > 1)
                {
                    //Idle Healer follows the pack
                    goalX = (int)myTeam.Average(u => GetUnitCoords(u, world).Item1) + 2;
                    goalY = (int)myTeam.Average(u => GetUnitCoords(u, world).Item2);
                }
            }

            //Executes the movement by getting A star Fetch A* path and updating the world based on it.
            var path = logic.GetPath(world, unit, goalX, goalY, currentTeam);
            if (path.Count > 1)
            {
                unit.CurrentVisualPath = new List<World.Tile>(path);
                unit.PathType = "move";

                World.Tile destination = world.Grid[startX, startY];
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    if (path[i].OccupyingUnit == null)
                    {
                        destination = path[i];
                        break;
                    }
                }

                if (destination != world.Grid[startX, startY])
                {
                    world.Grid[startX, startY].OccupyingUnit = null;
                    destination.OccupyingUnit = unit;
                }
            }
        }

        //Executes either a healing or attack action and handles the death of the unit.
        //Also updates the text in the console.
        private void PerformAction(Figurine actor, Figurine receiver, World.GameWorld world)
        {
            int oldHP = receiver.FigurineHealthCurrent;
            if (actor.UnitType == GameLogic.UnitType.Healer)
            {
                actor.FigurineHeal(actor, receiver);
                int healed = receiver.FigurineHealthCurrent - oldHP;
                Console.WriteLine($"    [HEAL] {actor.FigurineName} healed {receiver.FigurineName} for {healed} HP.");
            }
            else
            {
                actor.FigurineAttack(actor, receiver);
                int damage = oldHP - receiver.FigurineHealthCurrent;
                Console.WriteLine($"    [ATTACK] {actor.FigurineName} hit {receiver.FigurineName} for {damage} dmg. (Remaining: {receiver.FigurineHealthCurrent})");

                if (receiver.FigurineIsDead)
                {
                    Console.WriteLine($"    [DEATH] {receiver.FigurineName} was slain!");
                    (int rx, int ry) = GetUnitCoords(receiver, world);
                    if (rx != -1) world.Grid[rx, ry].OccupyingUnit = null;
                }
            }
        }

        //HELPER METHODS

        //Checks if action is within range, based on manhattan distance, range is hard-coded in this project, where archer has 3 tile range, which is sub-optimal.
        private bool IsInActionRange(Figurine unit, Figurine target, World.GameWorld world)
        {
            (int x1, int y1) = GetUnitCoords(unit, world);
            (int x2, int y2) = GetUnitCoords(target, world);
            if (x1 == -1 || x2 == -1) return false;
            int dist = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
            return unit.UnitType == GameLogic.UnitType.Archer ? dist <= 3 : dist <= 1;
        }

        //A rather inefficient way to find the specific actor's coordinates (design oversight)
        private (int, int) GetUnitCoords(Figurine unit, World.GameWorld world)
        {
            for (int x = 0; x < world.WorldWidth; x++)
                for (int y = 0; y < world.WorldHeight; y++)
                    if (world.Grid[x, y].OccupyingUnit == unit) return (x, y);
            return (-1, -1);
        }

        //A rather inefficient way to find all of the existing figurines (another design oversight)
        private List<Figurine> GetAllUnits(World.GameWorld world)
        {
            var units = new List<Figurine>();
            for (int x = 0; x < world.WorldWidth; x++)
                for (int y = 0; y < world.WorldHeight; y++)
                    if (world.Grid[x, y].OccupyingUnit != null) units.Add(world.Grid[x, y].OccupyingUnit);
            return units;
        }

        //Helper function, primarely for archers
        //Locates the closest tile that satisfies the unit's range requirements
        //while minimizing travel distance.
        private (int, int) FindOptimalRangeTile(World.GameWorld world, int sx, int sy, int tx, int ty, int range)
        {
            //If actor is already at the perfect range, stay put.
            int currentDist = Math.Abs(sx - tx) + Math.Abs(sy - ty);
            if (currentDist == range) return (sx, sy);

            (int bestX, int bestY) = (sx, sy);
            double closestDistToMe = double.MaxValue;

            //Scan a box around the target to find tiles at 'range' distance
            //and look at every tile that COULD be 3 steps away from the target
            for (int x = tx - range; x <= tx + range; x++)
            {
                for (int y = ty - range; y <= ty + range; y++)
                {
                    // Bounds check
                    if (x < 0 || x >= world.WorldWidth || y < 0 || y >= world.WorldHeight) continue;

                    // Check if this tile is exactly the 'range' (Manhattan distance)
                    if (Math.Abs(x - tx) + Math.Abs(y - ty) == range)
                    {
                        //Checks if tiles are walkable.
                        if (world.Grid[x, y].Type == TileType.Water ||
                            world.Grid[x, y].Type == TileType.Mountain) continue;

                        //Check if the tile is occupied by soemthing else.
                        if (world.Grid[x, y].OccupyingUnit != null) continue;

                        //To break the tie, choose the tile closest to the unit's current position to conserve movement.
                        //Euclidian distance cheat...
                        double distToMe = Math.Sqrt(Math.Pow(x - sx, 2) + Math.Pow(y - sy, 2));
                        if (distToMe < closestDistToMe)
                        {
                            closestDistToMe = distToMe;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }
            }

            return (bestX, bestY);
        }

        //Helper method for defender to find the choke points.
        //Choke point calculation is based on finding the walkability of the tiles.
        //Due to the set size of the map being more wide than tall, it searches the center.
        //Good enough for this assignment with mountains in the middle.
        private (int x, int y) FindChokePoint(World.GameWorld world, Figurine unit, GameLogic logic)
        {
            int scanX = world.WorldWidth / 2;
            int bestY = world.WorldHeight / 2;
            int maxObstacles = -1;
            for (int y = 0; y < world.WorldHeight; y++)
            {
                if (IsImpassable(world.Grid[scanX, y], unit)) continue;
                int obstacles = 0;
                if (scanX > 0 && IsImpassable(world.Grid[scanX - 1, y], unit)) obstacles++;
                if (scanX < world.WorldWidth - 1 && IsImpassable(world.Grid[scanX + 1, y], unit)) obstacles++;
                if (obstacles > maxObstacles) { maxObstacles = obstacles; bestY = y; }
            }
            return (scanX, bestY);
        }

        private bool IsImpassable(World.Tile tile, Figurine unit)
        {
            if (unit.UnitType == GameLogic.UnitType.Flier) return false;
            return tile.Type == TileType.Water || tile.Type == TileType.Mountain;
        }
    }


}