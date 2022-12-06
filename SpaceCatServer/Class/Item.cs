using SpaceCatServer.Class.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Rarity Rarity { get; set; }
        public int Essense { get; set; }
        public int Reputation { get; set; }

        internal Item(string name, string description, Rarity rarity, int essense, int reputation)
        {
            Name = name;
            Description = description;
            Rarity = rarity;
            Essense = essense;
            Reputation = reputation;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Item() { }
    }
}
