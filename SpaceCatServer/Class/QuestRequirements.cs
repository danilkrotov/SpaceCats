using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class QuestRequirement
    {
        public int Id { get; private set; }
        public QuestGroup QuestGroup { get; private set; }
        public Item Item { get; private set; }
        public int Count { get; private set; }

        /// <summary>
        /// Создание новый требований для квеста
        /// </summary>
        internal QuestRequirement(QuestGroup questGroup, Item item, int count)
        {
            QuestGroup = questGroup;
            Item = item;
            Count = count;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private QuestRequirement() { }
    }
}
