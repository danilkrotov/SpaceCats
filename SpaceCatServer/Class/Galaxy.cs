using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Galaxy
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Sector StartSector { get; private set; }
        /// <summary>
        /// Неуязвимая галактика? не будет очищена из бд таблицы секторов при перезапуске мира
        /// </summary>
        public bool IsInvulnerable { get; private set; }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Galaxy() { }

        /// <summary>
        /// Для автогенерации имени
        /// </summary>
        internal Galaxy(Sector startSector, bool isInvulnerable = false)
        {
            Name = RandomGroupSectorName();
            StartSector = startSector;
            IsInvulnerable = isInvulnerable;
            Save();
        }

        /// <summary>
        /// Для конкретного указания имени сектора
        /// </summary>
        internal Galaxy(string name, Sector startSector, bool isInvulnerable = false)
        {
            Name = name;
            StartSector = startSector;
            IsInvulnerable = isInvulnerable;
            Save();
        }

        /// <summary>
        /// Задаёт стартовый сектор для этой группы секторов
        /// </summary>
        public void SetStartSector(Sector sector) 
        {
            using (var db = new DataBaseContext())
            {
                Sector sec = db.Sectors.FirstOrDefault(s => s.Id == sector.Id); //ищем сектор
                Galaxy grSec = db.Galaxys.FirstOrDefault(s => s.Id == Id); //добавляем его как стартовый
                if (sec == null || grSec == null)
                {
                    throw new Exception("Исключение: Не найдена галактика или сектор для назначения его стартовым. Ожидается что галактика уже создана и мы присваеваем стартовый сектор");
                }
                else
                {
                    grSec.StartSector = sec;
                    db.SaveChanges();
                }
            }
        }

        public void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.StartSector = db.Sectors.FirstOrDefault(p => p.Id == this.StartSector.Id); //пробрасываем ссылку на Sector
                db.Galaxys.Add(this);
                db.SaveChanges();
            }
        }

        private string RandomGroupSectorName()
        {
            Random random = new Random();
            string sectorName = "NGC-";
            for (int i = 0; i < 3; i++)
            {
                sectorName += (char)random.Next(48, 57);
            }
            return sectorName;
        }

        public static Galaxy? GetGalaxyByName(string name) 
        {
            using (var db = new DataBaseContext())
            {
                return db.Galaxys.Include(z => z.StartSector).FirstOrDefault(s => s.Name.ToLower() == name.ToLower());
            }
        }
    }
}
