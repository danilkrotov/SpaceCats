using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;

namespace SpaceCatServer.Class
{
    internal class Character
    {
        public int Id { get; private set; }
        public Account Account { get; private set; }
        public string Name { get; private set; }
        public int Strenght { get; private set; }
        public Map Map { get; private set; }
        public int MovePoint { get; private set; }
        public int MaxMovePoint { get; private set; }
        public Fraction Fraction { get; private set; }
        public int Reputation { get; private set; }
        public Quest? ActiveQuest { get; private set; }
        /// <summary>
        /// Активный персонаж? все действия выполняются от имени активного персонажа
        /// </summary>
        public bool IsActive { get; private set; }

        internal Character(Account account, string name, CharacterOptions options)
        {
            Account = account;
            Name = name;
            Strenght = options.Strenght;
            //Sector - перенесён в Save т.к. присваивается сектор созданный из бд
            MovePoint = options.MovePoint;
            MaxMovePoint = options.MaxMovePoint;
            Fraction = options.Fraction;
            Reputation = options.Reputation;
            ActiveQuest = null;
            IsActive = options.IsActive;
            Save();
        }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Character() { }
        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.Account = db.Accounts.FirstOrDefault(p => p.Id == Account.Id); //пробрасываем ссылку на Account
                this.Map = db.Maps.Include(z => z.Sector).Include(z => z.Sector.Tile).FirstOrDefault(s => s.Id == Map.GetNeutralCapital().Id); //пробрасываем ссылку на столицу нейтралов
                this.Fraction = db.Fractions.FirstOrDefault(s => s.Id == Fraction.Id); //пробрасываем ссылку на фракцию
                db.Characters.Add(this);
                db.SaveChanges();
            }
        }
        private void Update()
        {
            using (var db = new DataBaseContext())
            {
                this.Account = db.Accounts.FirstOrDefault(p => p.Id == Account.Id); //пробрасываем ссылку на Account
                this.Map = db.Maps.Include(z => z.Sector).Include(z => z.Sector.Tile).FirstOrDefault(s => s.Id == Map.Id); //пробрасываем ссылку на столицу нейтралов
                this.Fraction = db.Fractions.FirstOrDefault(s => s == Fraction); //пробрасываем ссылку на фракцию
                if (this.ActiveQuest != null) { this.ActiveQuest = db.Quests.FirstOrDefault(p => p.Id == ActiveQuest.Id); } //если квест есть то пробрасываем на него ссылку
                db.Characters.Update(this);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Возвращает список всех персонажей на этом аккаунте
        /// </summary>
        public void GetQuest(Quest quest)
        {
            using (var db = new DataBaseContext())
            {
                this.ActiveQuest = quest;
                Update();
            }
        }
        /// <summary>
        /// Возвращает список всех персонажей на этом аккаунте
        /// </summary>
        public static List<Character> GetAllCharactersInAccount(string discordId) 
        {
            using (var db = new DataBaseContext())
            {
                List<Character> listChar = new List<Character>();
                foreach (Character chars in db.Characters.AsQueryable().Include(z => z.Account).Include(z => z.Map).Include(z => z.Map.Sector).Where(s => s.Account.Did == discordId))
                {
                    listChar.Add(chars);
                }
                return listChar;
            }
        }
        /// <summary>
        /// Делает персонажа активным, деактивирует оставшихся персонажей
        /// </summary>
        public void Activate(string name)
        {
            List<Character> lstChar = GetAllCharactersInAccount(Account.Did);
            for (int i = 0; i < lstChar.Count; i++)
            {
                lstChar[i].IsActive = false;
                lstChar[i].Update();
            }

            this.IsActive = true;
            Update();
        }
        /// <summary>
        /// Возвращает активного персонажа
        /// </summary>
        public static Character? GetActiveCharacter(string accountId) 
        {
            using (var db = new DataBaseContext())
            {
                return db.Characters
                    .Include(z => z.Account)
                    .Include(z => z.Fraction)
                    .Include(z => z.Map)
                    .Include(z => z.Map.Sector)
                    .Include(z => z.Map.Sector.Tile)
                    .Include(z => z.ActiveQuest)
                    .Include(z => z.ActiveQuest.QuestGroup)
                    .FirstOrDefault(s => s.Account.Did == accountId && s.IsActive == true);
            }
        }
        /// <summary>
        /// Перемещает персонажа в выбранный сектор
        /// </summary>
        public MoveError Move(string sectorName)
        {
            //Поверяем энергию, если не хватает ретурним тут

            //Проверяем если имя сектора в секторе в котором мы уже находимся, то ошибка
            if (sectorName.ToLower() == Map.Sector.Name.ToLower()) 
            {
                return MoveError.SelfSector;
            }

            //Выводим вокруг него доступные сектора
            List<Map> maps = Map.AllSectorAround(this.Map.Sector.Id);

            //Успех            
            for (int i = 0; i < maps.Count; i++)
            {
                if (sectorName.ToLower() == maps[i].Sector.Name.ToLower())
                {
                    //обновляем сектор персонажа
                    UpdateMap(maps[i]);
                    //добавляем сектор в видимые
                    SeeInFog(this.Account, this.Map, this.Map.IsInvulnerable);
                    return MoveError.Success; //лучше null
                }
            }

            //Иначе не найден такой сектор
            return MoveError.NotFound;
        }

        public void UpdateMap(Map map) 
        {
            this.Map = map;
            Update();
        }

        public void SeeInFog(Account account, Map map, bool isInvulnerable = false) 
        {
            MapFog? mapFog = MapFog.GetMapFog(account.Did, map.Id);
            //Данная локация ещё не открыта
            if (mapFog == null) 
            {
                new MapFog(account, map, isInvulnerable);
            }            
        }

        /// <summary>
        /// Перемещает персонажа в выбранную галактику
        /// </summary>
        public MoveError Warp(string galaxyName)
        {
            //Поверяем энергию, если не хватает ретурним тут

            //Если персонаж не стоит на тайле ворот
            if (this.Map.Sector.Tile.Types != Tile.Type.Gate)
            {
                return MoveError.GateNotFound;
            }

            //Проверить данное имя галактики на существование
            Galaxy? gal = Galaxy.GetGalaxyByName(galaxyName.ToLower());
            if (gal == null)
            {
                return MoveError.NotFound;
            }

            //обновляем сектор персонажа
            UpdateMap(Map.GetMap(gal.StartSector.Id));
            //добавляем сектор в видимые
            SeeInFog(this.Account, this.Map, this.Map.IsInvulnerable);
            return MoveError.Success;
        }

        /// <summary>
        /// Изменяет фракцию персонажа
        /// </summary>
        public void ChangeFraction(Fractions fractionName) 
        {
            using (var db = new DataBaseContext())
            {
                Fraction fr = db.Fractions.FirstOrDefault(p => p.Name == Enum.GetName(fractionName));
                if (fr == null)
                {
                    throw new Exception("Исключение: При смене фракции не найдена указанная фракция");
                }
                else 
                {
                    Fraction = fr;
                    ClearReputation(); // Обнуляет всю репутацию при переходе в новую фракцию
                    Update();
                }
            }
        }

        /// <summary>
        /// Добавляет репутации текущей фракции
        /// </summary>
        public void AddReputation(int reputation)
        {
            Reputation = Reputation + reputation;
            Update();
        }

        /// <summary>
        /// Очищает репутация у данной фракции
        /// </summary>
        public void ClearReputation()
        {
            Reputation = 0;
            Update();
        }

        /// <summary>
        /// Завершает квест у выбранного игрока
        /// </summary>
        public SellError QuestComplete()
        {
            using (var db = new DataBaseContext())
            {
                if (ActiveQuest == null)
                {
                    //у вас нет активного квеста
                    throw new Exception("Исключение: Проверка на наличие квеста уже включена в область !quest complete, была попытка сдать квест когда он не существует");
                }

                //Проверяем целостность квеста
                QuestGroup? qGroup = db.QuestGroups.FirstOrDefault(p => p.Id == ActiveQuest.QuestGroup.Id);

                if (qGroup == null)
                {
                    throw new Exception("Исключение: Ожидалось что группа квестов под этим ID существует, но это не так");
                }

                //Загружаем список требуемых предметов (если требований нет, то пропускаем)
                List<QuestRequirement> rec = db.QuestRequirements.Include(z => z.QuestGroup).Include(z => z.Item).Where(p => p.QuestGroup.Id == this.ActiveQuest.QuestGroup.Id).ToList();
                
                //Если требования по квесту были, то проверяем их
                if (rec != null) 
                {
                    //Проверяем есть ли в инвентаре нужные вещи в нужном количестве
                    for (int i = 0; i < rec.Count; i++)
                    {
                        //Возвращает один предмет, может быть пустым если предмета
                        Inventory? inv = db.Inventories.Include(z => z.Item).FirstOrDefault(p => p.Character.Id == this.Id && p.Item.Id == rec[i].Item.Id);
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

                    //Удаляем предметы (если требования были)
                    for (int i = 0; i < rec.Count; i++)
                    {
                        //Возвращает один предмет, может быть пустым если предмета
                        Inventory? inv = db.Inventories.Include(z => z.Item).FirstOrDefault(p => p.Character.Id == this.Id && p.Item.Id == rec[i].Item.Id);
                        if (inv == null)
                        {
                            throw new Exception("Исключение: Ожидается что предмет для выполнения задания существует и его можно удалить");
                        }

                        //Если предмет = количеству, строка будет удалена
                        if (rec[i].Count == inv.Count)
                        {
                            Inventory.DeleteItem(inv);
                        }

                        //Если количество меньше текущего, убавляем количество
                        if (rec[i].Count < inv.Count)
                        {
                            inv.Count = inv.Count - rec[i].Count;
                            db.SaveChanges();
                        }
                    }
                }

                //Начисляем предметы
                //Загружаем награды которые будут выданы игроку
                List<QuestReward> rew = db.QuestRewards.Include(z => z.Item).Where(p => p.QuestGroup.Id == qGroup.Id).ToList();

                for (int i = 0; i < rew.Count; i++)
                {
                    Inventory.AddItem(this, rew[i].Item, rew[i].Count);
                }

                if (this.ActiveQuest.EssenceReward > 0) 
                {
                    Account.AddEssence(this.Account.Did, this.ActiveQuest.EssenceReward);
                }

                if (this.ActiveQuest.ReputationReward > 0)
                {
                    this.Reputation = this.Reputation + this.ActiveQuest.ReputationReward;
                    db.SaveChanges();
                }

                db.Entry(this).State = EntityState.Modified;
                this.ActiveQuest = null;
                db.SaveChanges();

                return SellError.Success;
            }
        }
    }
}
