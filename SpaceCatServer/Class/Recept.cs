using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Recept
    {
        public int Id { get; private set; }
        public ReceptGroup ReceptGroup { get; private set; }
        public Item Item { get; private set; }
        public int Count { get; private set; }

        /// <summary>
        /// Создание нового рецепта крафта
        /// </summary>
        internal Recept(ReceptGroup receptGroup, Item item, int count)
        {
            ReceptGroup = receptGroup;
            Item = item;
            Count = count;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Recept() { }
    }
}
