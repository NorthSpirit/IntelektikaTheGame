using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelektikaTheGame.GameLogic;

namespace IntelektikaTheGame.GameLogic
{
    internal class FlowLogic
    {
        public GameLogic.player CurrentTurn { get; private set; } = GameLogic.player.Attacker;
        public bool IsGameOver { get; private set; } = false;

        private List<Figurine> _hasActed = new List<Figurine>();
        private int _turnCount = 1;

        public void ProcessTurn(World.GameWorld world, GameLogic logic)
        {
            if (IsGameOver) return;

            //1. Finds the units of my correct player's team (defender's units for defender, attackers for attacker)
            var myUnits = GetAllUnits(world).Where(u => u.Owner == CurrentTurn).ToList();

            //2. Finds the first figurine/unit that hasn't acted this turn
            var unitToAct = myUnits.FirstOrDefault(u => !_hasActed.Contains(u));

            if (unitToAct != null)
            {
                //Clears the clutter of the indicators (the X thingies) for the other units.
                foreach (var u in GetAllUnits(world)) u.CurrentVisualPath?.Clear();

                //Initializes the brain for the unit.
                AIBrain brain = new AIBrain();

                //Prints out the new turn has started message in the console
                if (CurrentTurn == GameLogic.player.Attacker && _hasActed.Count == 0)
                    Console.WriteLine($"\n--- Turn {_turnCount} start ---");

                //Writes who is acting.
                Console.WriteLine($"  > {CurrentTurn}: '{unitToAct.FigurineName}' is acting...");

                //Executes logic for the unit
                brain.ExecuteUnitTurn(unitToAct, world, logic, CurrentTurn);
                _hasActed.Add(unitToAct);

                //Checks if the game is over (enemy units dead)
                CheckWinCondition(world);
            }
            else
            {
                //3. Checks if there are still units in the list, who haven't acted.
                Console.WriteLine($"--- End of {CurrentTurn}'s turn ---");
                _hasActed.Clear();

                if (CurrentTurn == GameLogic.player.Defender)
                    _turnCount++;

                CurrentTurn = (CurrentTurn == GameLogic.player.Attacker)
                              ? GameLogic.player.Defender
                              : GameLogic.player.Attacker;
            }
        }

        //Checks if and who won (based on the person, who still has units)
        private bool CheckWinCondition(World.GameWorld world)
        {
            var allUnits = GetAllUnits(world);
            bool attackerExists = allUnits.Any(u => u.Owner == GameLogic.player.Attacker);
            bool defenderExists = allUnits.Any(u => u.Owner == GameLogic.player.Defender);

            if (!attackerExists)
            {
                EndGame(GameLogic.player.Defender);
                return true;
            }
            if (!defenderExists)
            {
                EndGame(GameLogic.player.Attacker);
                return true;
            }

            return false;
        }

        //A print message once the the game ends in the console/logs.
        private void EndGame(GameLogic.player winner)
        {
            IsGameOver = true;
            Console.WriteLine("\n*********************************");
            Console.WriteLine("           GAME OVER             ");
            Console.WriteLine($"    WINNER: {winner} Team!");
            Console.WriteLine($"    TOTAL TURNS: {_turnCount}");
            Console.WriteLine("*********************************\n");
        }

        //Helper method to get all the units in the world
        private List<Figurine> GetAllUnits(World.GameWorld world)
        {
            var units = new List<Figurine>();
            for (int x = 0; x < world.WorldWidth; x++)
                for (int y = 0; y < world.WorldHeight; y++)
                    if (world.Grid[x, y].OccupyingUnit != null)
                        units.Add(world.Grid[x, y].OccupyingUnit);
            return units;
        }
    }
}