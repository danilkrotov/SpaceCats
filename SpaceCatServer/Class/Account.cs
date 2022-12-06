using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;

namespace SpaceCatServer.Class
{
    internal class Account
    {
        public int Id { get; set; }
        public string Did { get; private set; }
        public int Essence { get; private set; }
        public SecurityLevel SecurityLevel { get; private set; }
        public int CharacterSlot { get; private set; }
        public Locale Localization { get; private set; }
        internal Account(string did, AccountOptions accountOptions)
        {
            Did = did;
            Essence = accountOptions.StartEssence;
            SecurityLevel = SecurityLevel.User;
            CharacterSlot = 0;
            Localization = accountOptions.Localization;
            Save();
        }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Account() { }
        /// <summary>
        /// Сохранить в БД
        /// </summary>
        public void Save() 
        {
            using (var db = new DataBaseContext())
            {
                db.Accounts.Add(this);
                db.SaveChanges();
            }
        }
        /// <summary>
        /// Вовзращает экземпляр Account из БД
        /// </summary>
        public static Account? Get(string discordId) 
        {
            using (var db = new DataBaseContext())
            {
                return db.Accounts.FirstOrDefault(acc => acc.Did == discordId);
            }
        }

        public static void AddEssence(string discordId, int essence) 
        {
            using (var db = new DataBaseContext())
            {
                Account? acc = db.Accounts.FirstOrDefault(acc => acc.Did == discordId);
                if (acc == null)
                {
                    throw new Exception("Исключение: Ожидается что персонаж у которого мы смотрим инвентарь выбран по умолчанию, но персонаж не найден");
                }
                else 
                {
                    acc.Essence = acc.Essence + essence;
                    db.SaveChanges();
                }
            }
        }
    }
}
