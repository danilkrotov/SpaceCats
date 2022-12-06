using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Inventory
    {
        public int Id { get; set; }
        public Character Character { get; set; }
        public Item Item { get; set; }
        public int Count { get; set; }

        internal Inventory(Character character, Item item, int count)
        {
            Character = character;
            Item = item;
            Count = count;
            Save();
        }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Inventory() { }
        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.Character = db.Characters.FirstOrDefault(p => p.Id == Character.Id); //пробрасываем ссылку на Character
                this.Item = db.Items.FirstOrDefault(p => p.Id == Item.Id); //пробрасываем ссылку на Items
                db.Inventories.Add(this);
                db.SaveChanges();
            }
        }

        public static void AddItem(Character charct, Item item, int count) 
        {
            using (var db = new DataBaseContext())
            {
                //Проверяем есть ли такой предмет в базе данных у этого игрока (если есть то добавляем количество)
                Inventory inv = db.Inventories.FirstOrDefault(p => p.Character.Id == charct.Id && p.Item.Id == item.Id);
                if (inv == null)
                {
                    //нет предмета
                    new Inventory(charct, item, count);
                }
                else
                {
                    inv.Count = inv.Count + count;
                    db.SaveChanges();
                }
            }
        }

        public static void AddItemList(Character charct, List<LootBox> lootList) 
        {            
            using (var db = new DataBaseContext())
            {
                for (int i = 0; i < lootList.Count; i++)
                {
                    //Проверяем есть ли такой предмет в базе данных у этого игрока (если есть то добавляем количество)
                    Inventory inv = db.Inventories.FirstOrDefault(p => p.Character.Id == charct.Id && p.Item.Id == lootList[i].Item.Id);
                    if (inv == null)
                    {
                        //нет предмета
                        new Inventory(charct, lootList[i].Item, lootList[i].Count);
                    }
                    else 
                    {
                        inv.Count = inv.Count + lootList[i].Count;
                        db.SaveChanges();
                    }
                }
            }
        }

        public static List<Inventory> GetItem(Character? charct, int page)
        {
            using (var db = new DataBaseContext())
            {
                if (charct == null) 
                {
                    throw new Exception("Исключение: Ожидается что персонаж у которого мы смотрим инвентарь выбран по умолчанию, но персонаж не найден");
                }

                //Возвращает все предметы, может быть пустым если предметов нет
                return db.Inventories.Include(z => z.Item).Where(p => p.Character.Id == charct.Id).Skip(10 * page).Take(10).ToList();
            }
        }

        public static void DeleteItem(Inventory inv)
        {
            using (var db = new DataBaseContext())
            {                
                //Предмет и Персонаж уже должны существовать
                Inventory? invs = db.Inventories.FirstOrDefault(p => p.Id == inv.Id);
                if (inv == null)
                {
                    throw new Exception("Исключение: Ожидается что предмет существует и его можно удалить");
                }
                else
                {
                    db.Inventories.Remove(invs);
                    db.SaveChanges();
                }                
            }
        }

        public static SellError SellItem(Character? charct, string itemName, int count)
        {
            using (var db = new DataBaseContext())
            {
                if (charct == null)
                {
                    throw new Exception("Исключение: Ожидается что персонаж у которого мы смотрим инвентарь выбран по умолчанию, но персонаж не найден");
                }

                //Возвращает все предметы, может быть пустым если предметов нет
                Inventory? inv = db.Inventories.Include(z => z.Item).FirstOrDefault(p => p.Character.Id == charct.Id && p.Item.Name.ToLower() == itemName.ToLower());

                if (inv == null) 
                {
                    return SellError.NotFound;
                }
                //Если число которое мы хотим продать больше чем число в инвентаре
                if (count > inv.Count)
                {
                    return SellError.NotEnough;
                }

                //Продаем предмет
                //Если предмет = количеству, строка будет удалена
                if (count == inv.Count) 
                {
                    DeleteItem(inv);
                    //Начисляем ессенции
                    Account.AddEssence(charct.Account.Did, inv.Item.Essense * count);
                    charct.AddReputation(inv.Item.Reputation * count);
                    return SellError.Success;
                }

                //Если количество меньше текущего, убавляем количество
                if (count < inv.Count)
                {
                    inv.Count = inv.Count - count;
                    db.SaveChanges();
                    //Начисляем ессенции
                    Account.AddEssence(charct.Account.Did, inv.Item.Essense * count);
                    charct.AddReputation(inv.Item.Reputation * count);
                    return SellError.Success;
                }

                //Не удалось выполнить ни один if
                return SellError.Error;
            }
        }

        public static SellError CraftItem(Character? charct, string receptName) 
        {
            using (var db = new DataBaseContext())
            {
                if (charct == null)
                {
                    throw new Exception("Исключение: Ожидается что персонаж у которого мы смотрим инвентарь выбран по умолчанию, но персонаж не найден");
                }

                //Проверяем есть ли такой рецепт
                ReceptGroup? recGroup = db.ReceptGroups.FirstOrDefault(p => p.Name.ToLower() == receptName.ToLower());

                if (recGroup == null)
                {
                    return SellError.NotFound;
                }

                //Загружаем список предметов для крафта
                List<Recept> rec = db.Recepts.Include(z => z.Item).Where(p => p.ReceptGroup.Id == recGroup.Id).ToList();
                if (rec == null)
                {
                    throw new Exception("Исключение: Ожидается список предметов для данного крафта существует");
                }

                //Проверяем есть ли в инвентаре нужные вещи в нужном количестве
                for (int i = 0; i < rec.Count; i++)
                {
                    //Возвращает один предмет, может быть пустым если предмета
                    Inventory? inv = db.Inventories.Include(z => z.Item).FirstOrDefault(p => p.Character.Id == charct.Id && p.Item.Id == rec[i].Item.Id);
                    if (inv == null)
                    {
                        return SellError.NotFound;
                    }

                    //Если предмета в инвентаре меньше, чем нужно для крафта
                    if (inv.Count < rec[i].Count) 
                    {
                        return SellError.NotEnough;
                    }
                }

                //Удаляем предметы
                for (int i = 0; i < rec.Count; i++)
                {
                    //Возвращает один предмет, может быть пустым если предмета
                    Inventory? inv = db.Inventories.Include(z => z.Item).FirstOrDefault(p => p.Character.Id == charct.Id && p.Item.Id == rec[i].Item.Id);

                    //Если предмет = количеству, строка будет удалена
                    if (rec[i].Count == inv.Count)
                    {
                        DeleteItem(inv);
                    }

                    //Если количество меньше текущего, убавляем количество
                    if (rec[i].Count < inv.Count)
                    {
                        inv.Count = inv.Count - rec[i].Count;
                        db.SaveChanges();
                    }
                }

                //Начисляем предметы
                //Загружаем списко предметов которые будут созданы
                List<ReceptReward> rew = db.Rewards.Include(z => z.Item).Where(p => p.ReceptGroup.Id == recGroup.Id).ToList();
                
                for (int i = 0; i < rew.Count; i++)
                {
                    AddItem(charct, rew[i].Item, rew[i].Count);
                }
                
                return SellError.Success;
            }
        }
    }
}
