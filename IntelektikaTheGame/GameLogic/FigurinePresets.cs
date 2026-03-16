using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntelektikaTheGame.GameLogic.GameLogic;

namespace IntelektikaTheGame.GameLogic
{
    namespace IntelektikaTheGame.GameLogic
    {
        internal class FigurinePresets
        {
            public static Figurine CreateUnit(UnitType type, player owner)
            {
                //Some presets for units to populate the map, to save on time, note cavalry uses AI of a melee.
                Figurine unit = type switch
                {
                    UnitType.Melee => new Figurine
                    {
                        FigurineName = owner == player.Defender ? "Defender" : "Shock Trooper",
                        FigurineHealthMax = 60,
                        FigurineAttackPower = 12,
                        FigurineArmor = 8,
                        FigurineMovementPointsMax = 4,
                        picRef = owner == player.Defender ? "def_melee" : "att_melee"
                    },
                    UnitType.Archer => new Figurine
                    {
                        FigurineName = owner == player.Defender ? "Bowman" : "Archer",
                        FigurineHealthMax = 30,
                        FigurineAttackPower = 19,
                        FigurineArmor = 2,
                        FigurineMovementPointsMax = 6,
                        picRef = owner == player.Defender ? "def_archer" : "att_archer"
                    },
                    UnitType.Flier => new Figurine
                    {
                        FigurineName = owner == player.Defender ? "Wyvern Rider" : "Dark Rider",
                        FigurineHealthMax = 45,
                        FigurineAttackPower = 22,
                        FigurineArmor = 4,
                        FigurineMovementPointsMax = 8,
                        picRef = owner == player.Defender ? "def_flier" : "att_flier"
                    },
                    UnitType.Healer => new Figurine
                    {
                        FigurineName = owner == player.Defender ? "Priestess" : "Witch",
                        FigurineHealthMax = 25,
                        FigurineMagicPower = 15,
                        FigurineArmor = 4,
                        FigurineMovementPointsMax = 5,
                        picRef = owner == player.Defender ? "def_healer" : "att_healer"
                    },
                    //Cavalry uses Melee AI but has unique stats and sprites
                    _ when type.ToString() == "Cavalry" || true => new Figurine
                    {
                        FigurineName = owner == player.Defender ? "Leitis" : "Crusader",
                        FigurineHealthMax = 70,
                        FigurineAttackPower = 14,
                        FigurineArmor = 6,
                        FigurineMovementPointsMax = 10,
                        picRef = owner == player.Defender ? "def_cav" : "att_cav",
                        UnitType = UnitType.Melee
                    }
                };

                //Set shared fields
                unit.Owner = owner;
                unit.FigurineHealthCurrent = unit.FigurineHealthMax;

                //Catch the type, if it wasn't in the switch
                if (unit.UnitType == default) unit.UnitType = type;

                return unit;
            }
        }
    }
}
