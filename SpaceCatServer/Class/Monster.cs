using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Class.MiniClass;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SpaceCatServer.Class.Tile;

namespace SpaceCatServer.Class
{
    internal class Monster
    {
        public int Id { get; private set; }
        public Tile MonsterTile { get; private set; }
        public int Level { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public LootBox LootBox { get; private set; }

        internal Monster(Tile monsterId, int level, string name, string description, LootBox lootBox)
        {
            MonsterTile = monsterId;
            Level = level;
            Name = name;
            Description = description;
            LootBox = lootBox;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Monster() { }

        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                MonsterTile = db.Tiles.FirstOrDefault(p => p == MonsterTile);
                LootBox = db.LootBoxes.FirstOrDefault(s => s.Id == LootBox.Id);
                db.Monsters.Add(this);
                db.SaveChanges();
            }
        }

        public static FightAnswer Fight(Character charct)
        {
            using (var db = new DataBaseContext())
            {
                //подготавливаем новый ответ
                FightAnswer fightAnswer = new FightAnswer();
                //ищем монстра по ID тайла
                Monster? mob = db.Monsters.Include(z => z.MonsterTile).Include(z => z.LootBox).Include(z => z.LootBox.Item).FirstOrDefault(p => p.MonsterTile.ModelId == charct.Map.Sector.Tile.ModelId);
                if (mob == null)
                {
                    throw new Exception("Исключение: Персонаж  " + charct.Name + " начал сражение с монстром, но монстра с ID: " + charct.Map.Sector.Tile.ModelId + " не оказалось в базе");
                }
                else
                {
                    // Сохраняем монстра с которым сражались
                    fightAnswer.MonsterInfo = mob;
                    //проверяем атаку персонажа и атаку монстра
                    if (charct.Strenght >= mob.Level)
                    {
                        //заменить монстра в этой клетке на пустоту
                        charct.Map.Sector.EmptyThisTile();
                        //выдать награду за бой
                        //Получаем весь дроп с монстра
                        List<LootBox> lstItems = db.LootBoxes.Include(z => z.Item).Include(z => z.LootBoxGroup).Where(w => w.LootBoxGroup.Id == mob.LootBox.Id).ToList();
                        //Бросаем кубик на каждую позицию
                        Random rnd = new Random();
                        for (int i = 0; i < lstItems.Count; i++)
                        {
                            //бросаем кубик от 1 до 10 0000
                            int dice = rnd.Next(1, 10000);
                            //Если кубик больше шанса то выдаём лут
                            if (lstItems[i].Chance >= dice)
                            {
                                fightAnswer.ItemList.Add(lstItems[i]);
                                //Сохраняем в инвентарь

                            }
                        }
                        //победа
                        fightAnswer.FightError = FightError.Victory;
                        return fightAnswer;
                    }
                    else
                    {
                        //перезаписать персонажа в сектор N007
                        charct.UpdateMap(Map.GetNeutralCapital());
                        //поражение
                        fightAnswer.FightError = FightError.Defeat;
                        return fightAnswer;
                    }
                }
            }
        }
    }
}
