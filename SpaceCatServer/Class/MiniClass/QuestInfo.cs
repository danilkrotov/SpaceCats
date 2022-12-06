using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class.MiniClass
{
    internal class QuestInfo
    {
        public Quest? Quest { get; set; }
        public List<QuestReward>? QuestRewards { get; set; }
        public List<QuestRequirement>? QuestRequirements { get; set; }
        public QuestGroup? QuestGroup { get; set; }
    }
}
