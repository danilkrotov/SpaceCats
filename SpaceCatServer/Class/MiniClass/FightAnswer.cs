using SpaceCatServer.Class.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class.MiniClass
{
    internal class FightAnswer
    {
        public FightError FightError { get; set; }
        public List<LootBox> ItemList { get; set; } = new List<LootBox>();
        public Monster MonsterInfo { get; set; }
    }
}
