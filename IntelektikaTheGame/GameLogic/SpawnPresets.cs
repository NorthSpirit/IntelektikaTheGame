using IntelektikaTheGame.GameLogic.IntelektikaTheGame.GameLogic;
using IntelektikaTheGame.World;
using static IntelektikaTheGame.GameLogic.GameLogic;

namespace IntelektikaTheGame.GameLogic
{
    internal class SpawnPresets
    {
        public static void SpawnTeams(GameWorld world)
        {
            // ATTACKERS (Left Side - X around 5)
            PlaceUnit(world, 5, 10, UnitType.Melee, player.Attacker);
            PlaceUnit(world, 5, 15, UnitType.Melee, player.Attacker);
            PlaceUnit(world, 4, 12, UnitType.Archer, player.Attacker);
            PlaceUnit(world, 4, 18, UnitType.Healer, player.Attacker);
            PlaceUnit(world, 3, 15, UnitType.Flier, player.Attacker);

            // DEFENDERS (Right Side - X around 45)
            PlaceUnit(world, 45, 10, UnitType.Melee, player.Defender);
            PlaceUnit(world, 45, 15, UnitType.Melee, player.Defender);
            PlaceUnit(world, 46, 12, UnitType.Archer, player.Defender);
            PlaceUnit(world, 46, 18, UnitType.Healer, player.Defender);
            PlaceUnit(world, 47, 15, UnitType.Flier, player.Defender);
        }

        private static void PlaceUnit(GameWorld world, int x, int y, UnitType type, player owner)
        {
            Figurine unit = FigurinePresets.CreateUnit(type, owner);

            if (x >= 0 && x < world.WorldWidth && y >= 0 && y < world.WorldHeight)
            {
                world.Grid[x, y].OccupyingUnit = unit;
            }
        }
    }
}