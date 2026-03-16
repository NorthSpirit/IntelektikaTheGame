using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Basic class for a generic unit/actor
namespace IntelektikaTheGame.GameLogic
{
    internal class Figurine
    {
        public int FigurineHealthMax {  get; set; }
        //Name exists partly for flavouring and partly to be able to tell what is what in the console logs
        public string FigurineName { get; set; }
        public int FigurineHealthCurrent { get; set; }
        public int FigurineAttackPower { get; set; }
        //Magic power is used by healers.
        public int FigurineMagicPower { get; set; }
        public int FigurineArmor {  get; set; }
        public int FigurineMovementPointsMax { get; set; }
        public int FigurineMovementPointsMin { get; set; }
        
        //Checks if the unit is dead for future deletion
        public bool FigurineIsDead => FigurineHealthCurrent <= 0;
        
        //A picture reference for the MonoGame to render
        public string? picRef {  get; set; } = string.Empty;

        //Checks, to whom the unit belongs to, which is important 
        public GameLogic.player Owner { get; set; }

        //Checks the unit type for their AI (and range in case of archer)
        public GameLogic.UnitType UnitType { get; set; }
        //Saves the path for the visual display of indicators for visual clarity
        public List<World.Tile> CurrentVisualPath { get; set; } = new List<World.Tile>();
        //To check, what order was issued - attack, move or heal.
        public string PathType { get; set; }

        internal void FigurineAttack(Figurine attacker, Figurine defender)
        {
            //Basically, a random number.
            double k = 0.07;
            double reductionPercent = (defender.FigurineArmor * k) / (1 + defender.FigurineArmor * k);
            int damageDealt = Math.Max((int)(attacker.FigurineAttackPower * (1 - reductionPercent)), 1);
            defender.FigurineHealthCurrent -= damageDealt;
            //No overkilling.
            if (defender.FigurineHealthCurrent < 0)
                defender.FigurineHealthCurrent = 0;
        }

        internal void FigurineHeal(Figurine caster, Figurine receiver)
        {
            if (receiver.FigurineIsDead) return;
            Random rand = new Random();
            double multiplier = rand.NextDouble() * (1.21 - 0.89) + 0.89;
            int healAmount = (int)(caster.FigurineMagicPower * multiplier);
            receiver.FigurineHealthCurrent += healAmount;
            //No overhealing.
            if (receiver.FigurineHealthCurrent > receiver.FigurineHealthMax)
                receiver.FigurineHealthCurrent = receiver.FigurineHealthMax;
        }
    }
}
