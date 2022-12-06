using SpaceCatServer.Class.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class CharacterOptions
    {
        public Fraction Fraction = Fraction.GetDefaultFraction();
        public int Reputation = 0;
        public int Strenght = 0;
        public int MovePoint = 3;
        public int MaxMovePoint = 3;
        public bool IsActive = false;
    }
}
