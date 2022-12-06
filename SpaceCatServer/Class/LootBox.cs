using Microsoft.EntityFrameworkCore.Diagnostics;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    /// <summary>
    /// Шанс выпадения рассмотрен как случайное значение от 1 до 10 000. На каждый предмет в списке проверяется шанс.
    /// </summary>
    internal class LootBox
    {
        public int Id { get; set; }
        public LootBoxGroup LootBoxGroup { get; set; }
        public Item Item { get; set; }
        /// <summary>
        /// Не должен привышать 10000 (0.01%)
        /// </summary>
        public int Chance { get; set; }
        public int Count { get; set; }

        /// <summary>
        /// Конструктор - Список предметов + шансов
        /// </summary>
        internal LootBox(Item item, int chance, int count)
        {
            Item = item;
            Chance = chance;
            Count = count;
        }

        /// <summary>
        /// Создание новой группы предметов
        /// </summary>
        internal LootBox(LootBoxGroup lootBoxGroup, Item item, int chance, int count)
        {
            LootBoxGroup = lootBoxGroup;
            Item = item;
            Chance = chance;
            Count = count;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private LootBox() { }

        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.LootBoxGroup = db.LootBoxGroups.FirstOrDefault(s => s.Id == LootBoxGroup.Id);
                this.Item = db.Items.FirstOrDefault(s => s.Id == Item.Id);
                db.LootBoxes.Add(this);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Список предметов + шансов
        /// </summary>
        public static void Create(List<LootBox> listItem) 
        {
            using (var db = new DataBaseContext())
            {
                LootBoxGroup lbg = new LootBoxGroup();
                db.LootBoxGroups.Add(lbg);
                db.SaveChanges();            

                for (int i = 0; i < listItem.Count; i++)
                {
                    db.LootBoxes.Add(new LootBox(db.LootBoxGroups.FirstOrDefault(s => s.Id == lbg.Id), db.Items.FirstOrDefault(s => s.Id == listItem[i].Item.Id), listItem[i].Chance, listItem[i].Count));
                    db.SaveChanges();
                }
            }
        }
    }
}
