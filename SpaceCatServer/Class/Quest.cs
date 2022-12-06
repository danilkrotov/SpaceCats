using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Relational;
using SpaceCatServer.Class.MiniClass;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Quest
    {
        public int Id { get; private set; }
        /// <summary>
        /// Требования для выполнения квеста и награда
        /// </summary>
        public QuestGroup QuestGroup { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ReputationReward { get; private set; }
        public int EssenceReward { get; private set; }
        public Tile.Model Model { get; private set; }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Quest() { }
        internal Quest(QuestGroup questGroup, string name, string description, int reputationReward, int essenceReward, Tile.Model model)
        {
            QuestGroup = questGroup;
            Name = name;
            Description = description;
            ReputationReward = reputationReward;
            EssenceReward = essenceReward;
            Model = model;
        }

        /// <summary>
        /// Возвращает квест по его тайлу
        /// </summary>
        public static Quest Get(Tile.Model model)
        {
            using (var db = new DataBaseContext())
            {
                Quest? quest = db.Quests.FirstOrDefault(p => p.Model == model);
                if (quest == null)
                {
                    throw new Exception("Исключение: Ожидается что Квест с ModelID: " + model + " уже существует, но он не найден");
                }
                else
                {
                    return quest;
                }
            }
        }

        /// <summary>
        /// Возвращает описание квеста
        /// </summary>
        public static QuestInfo GetDescription(Tile.Model model) 
        {
            using (var db = new DataBaseContext())
            {
                Quest? quest = db.Quests.Include(z => z.QuestGroup).FirstOrDefault(p => p.Model == model);
                if (quest == null)
                {
                    throw new Exception("Исключение: Ожидается что Квест с ModelID: " + model + " уже существует, но он не найден");
                }
                else 
                {
                    QuestInfo questInfo = new QuestInfo();
                    questInfo.Quest = quest;
                    questInfo.QuestGroup = db.QuestGroups.FirstOrDefault(p => p.Id == quest.QuestGroup.Id);
                    questInfo.QuestRequirements = db.QuestRequirements.Include(z => z.QuestGroup).Include(z => z.Item).Where(p => p.QuestGroup.Id == quest.QuestGroup.Id).ToList();
                    questInfo.QuestRewards = db.QuestRewards.Include(z => z.QuestGroup).Include(z => z.Item).Where(p => p.QuestGroup.Id == quest.QuestGroup.Id).ToList();

                    return questInfo;
                }
            }
        }

        /// <summary>
        /// Возвращает описание квеста
        /// </summary>
        public static QuestInfo GetDescriptionByQuest(Quest quest)
        {
            using (var db = new DataBaseContext())
            {
                Quest? q = db.Quests.Include(z => z.QuestGroup).FirstOrDefault(p => p.Id == quest.Id);
                if (q == null)
                {
                    throw new Exception("Исключение: Ожидается что Квест уже существует, но он не найден");
                }
                else
                {
                    QuestInfo questInfo = new QuestInfo();
                    questInfo.Quest = q;
                    questInfo.QuestGroup = db.QuestGroups.FirstOrDefault(p => p.Id == q.QuestGroup.Id);
                    questInfo.QuestRequirements = db.QuestRequirements.Include(z => z.QuestGroup).Include(z => z.Item).Where(p => p.QuestGroup.Id == q.QuestGroup.Id).ToList();
                    questInfo.QuestRewards = db.QuestRewards.Include(z => z.QuestGroup).Include(z => z.Item).Where(p => p.QuestGroup.Id == q.QuestGroup.Id).ToList();

                    return questInfo;
                }
            }
        }

        /// <summary>
        /// Возвращае текст отформатированного описание задания
        /// </summary>
        public static string QuestInfo(QuestInfo qinfo) 
        {
            string qdescription = "";
            qdescription += "**" + qinfo.Quest.Name + "**\n";
            qdescription += qinfo.Quest.Description + "\n\n";
            qdescription += "Requirements:" + "\n";
            if (qinfo.QuestRequirements != null)
            {
                for (int i = 0; i < qinfo.QuestRequirements.Count; i++)
                {
                    qdescription += qinfo.QuestRequirements[i].Item.Name + " - " + qinfo.QuestRequirements[i].Count + "\n";
                }
            }
            else
            {
                qdescription += "No requirements" + "\n";
            }
            qdescription += "\n";
            qdescription += "Rewards:" + "\n";
            if (qinfo.QuestRewards != null)
            {
                for (int i = 0; i < qinfo.QuestRewards.Count; i++)
                {
                    qdescription += qinfo.QuestRewards[i].Item.Name + " - " + qinfo.QuestRewards[i].Count + "\n";
                }
            }

            if (qinfo.Quest.EssenceReward != 0)
            {
                qdescription += "Essence: " + qinfo.Quest.EssenceReward + "\n";
            }

            if (qinfo.Quest.ReputationReward != 0)
            {
                qdescription += "Reputation: " + qinfo.Quest.ReputationReward + "\n";
            }
            return qdescription;
        }

        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                db.Quests.Add(this);
                db.SaveChanges();
            }
        }
    }
}
