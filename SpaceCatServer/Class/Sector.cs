using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;

namespace SpaceCatServer.Class
{
    internal class Sector
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Tile Tile { get; private set; }
        /// <summary>
        /// Неуязвимый сектор? не будет очищен из бд таблицы секторов при перезапуске мира
        /// </summary>
        public bool IsInvulnerable { get; private set; }
        /// <summary>
        /// Для стартовой инициализации
        /// </summary>
        internal Sector (string name, Tile tile, bool isInvulnerable)
        {
            Name = name;
            Tile = tile;
            IsInvulnerable = isInvulnerable;
        }
        /// <summary>
        /// Для создания из кода
        /// </summary>
        internal Sector(Tile tile, bool isInvulnerable = false)
        {            
            Name = RandomGroupSectorName();
            Tile = tile;
            IsInvulnerable = isInvulnerable;
            Save();            
        }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private Sector() { }

        public void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.Tile = db.Tiles.FirstOrDefault(p => p.ModelId == this.Tile.ModelId); //пробрасываем ссылку на Sector
                db.Sectors.Add(this);
                db.SaveChanges();
            }
        }

        private void Update()
        {
            using (var db = new DataBaseContext())
            {
                this.Tile = db.Tiles.FirstOrDefault(p => p.ModelId == this.Tile.ModelId); //пробрасываем ссылку на Tile
                db.Sectors.Update(this);
                db.SaveChanges();
            }
        }

        private string RandomGroupSectorName()
        {
            Random random = new Random();
            string sectorName = "";
            for (int i = 0; i < 3; i++)
            {
                sectorName += (char)random.Next(65, 90);
            }
            sectorName += "-";
            for (int i = 0; i < 3; i++)
            {
                sectorName += (char)random.Next(48, 57);
            }
            return sectorName;
        }
        /// <summary>
        /// Создает случайный сектор и возвращает его
        /// </summary>
        public static Sector CreateRandomSector(int level) 
        {
            List<Tile> sectorsList = new List<Tile>();
            //Gate не создавать, ворота будут созданы при генерации

            if (level == 1)
            {
                sectorsList = GalaxyGeneration.GenerateLevel1();
            }

            Random rnd = new Random();
            Tile randomSector = sectorsList[rnd.Next(0, sectorsList.Count)]; //случайное значение из списка всех значений для уровня сектора
            return new Sector(randomSector);
        }

        /// <summary>
        /// Создает сектор с вратами для путешествий
        /// </summary>
        public static Sector CreateGateSector()
        {
            using (var db = new DataBaseContext())
            {
                Tile? tile = db.Tiles.FirstOrDefault(p => p.Types == Tile.Type.Gate);
                if (tile == null)
                {
                    throw new Exception("Исключение: Ожидается что Type Tile с вратами уже существует и добавлен в базу, однако это не так");
                }
                else 
                {
                    return new Sector(tile);
                }
            }
        }
        /// <summary>
        /// Возвращает Galaxy в котором находится этот сектор
        /// </summary>
        public Galaxy GetGalaxys() 
        {
            using (var db = new DataBaseContext())
            {
                Map map = db.Maps.Include(z => z.Galaxy).Include(z => z.Sector).FirstOrDefault(p => p.Sector.Id == this.Id);
                return map.Galaxy;
            }
        }
        /// <summary>
        /// Заменить текущий тайл на тайл пустого космоса
        /// </summary>
        public void EmptyThisTile()
        {
            using (var db = new DataBaseContext())
            {
                Tile? tile = db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty);
                if (tile == null)
                {
                    throw new Exception("Исключение: Ожидается что Type Tile с пустым космосом уже существует и добавлен в базу, однако это не так");
                }
                else
                {
                    this.Tile = tile;
                    Update();
                }
            }
        }
    }
}