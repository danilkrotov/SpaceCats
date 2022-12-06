using Microsoft.EntityFrameworkCore;
using SpaceCatServer.Class;
using SpaceCatServer.Class.Enums;

namespace SpaceCatServer.Database
{
    internal class DataBaseContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<Galaxy> Galaxys { get; set; }
        public DbSet<MapFog> MapFogs { get; set; }
        public DbSet<Tile> Tiles { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Monster> Monsters { get; set; }
        public DbSet<LootBox> LootBoxes { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<LootBoxGroup> LootBoxGroups { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<ReceptGroup> ReceptGroups { get; set; }
        public DbSet<Recept> Recepts { get; set; }
        public DbSet<ReceptReward> Rewards { get; set; }
        public DbSet<Fraction> Fractions { get; set; }
        public DbSet<QuestGroup> QuestGroups { get; set; }
        public DbSet<QuestReward> QuestRewards { get; set; }
        public DbSet<QuestRequirement> QuestRequirements { get; set; }

        public DataBaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("server=localhost;port=3305;database=spacecats;uid=root;password=vertrigo;");
            optionsBuilder.UseMySQL("server=localhost;database=spacecats;user=root;password=vertrigo");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Создаём базовые сектора
            modelBuilder.Entity<Tile>().HasData(
                new Tile[]
                {
                    new Tile(Tile.Model.NeutralCapital, Tile.Type.City),
                    new Tile(Tile.Model.GoodCapital, Tile.Type.City),
                    new Tile(Tile.Model.EvylCapital, Tile.Type.City),
                    new Tile(Tile.Model.Wall, Tile.Type.Empty),
                    new Tile(Tile.Model.Gate, Tile.Type.Gate),
                    new Tile(Tile.Model.Hidden, Tile.Type.Empty),
                    new Tile(Tile.Model.Empty, Tile.Type.Empty),
                    new Tile(Tile.Model.Quest1, Tile.Type.Quest),
                    new Tile(Tile.Model.Monster, Tile.Type.Monster),
                    new Tile(Tile.Model.Planet, Tile.Type.Empty),
                    new Tile(Tile.Model.Asteroid, Tile.Type.Empty),
                    new Tile(Tile.Model.Quest2, Tile.Type.Quest)
                });
        }

        public static void AfterInit() 
        {
            using (var db = new DataBaseContext())
            {
                //Создаём фракции
                if (db.Fractions.Count() == 0)
                {
                    db.Fractions.Add(new Fraction(Enum.GetName(Class.Enums.Fractions.Neutral)));
                    db.Fractions.Add(new Fraction(Enum.GetName(Class.Enums.Fractions.Good)));
                    db.Fractions.Add(new Fraction(Enum.GetName(Class.Enums.Fractions.Evyl)));
                    db.SaveChanges();
                }

                //Создаем сектора
                if (db.Sectors.Count() == 0) 
                {
                    db.Sectors.Add(new Sector("HEL-666", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.EvylCapital), true));
                    db.Sectors.Add(new Sector("ORI-042", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), true));
                    db.Sectors.Add(new Sector("ORI-358", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Gate), true));
                    db.Sectors.Add(new Sector("ORI-123", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), true));
                    db.Sectors.Add(new Sector("NEU-007", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.NeutralCapital), true));
                    db.Sectors.Add(new Sector("ORI-532", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), true));
                    db.Sectors.Add(new Sector("ORI-752", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), true));
                    db.Sectors.Add(new Sector("ORI-729", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), true));
                    db.Sectors.Add(new Sector("GOD-888", db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.GoodCapital), true));
                    db.SaveChanges();
                }

                //Ищем стартовую галактику
                Map mp = db.Maps.FirstOrDefault(p => p.Galaxy.Name == "Orion");
                //если изначальная галактика не создана, создаём
                if (mp == null) 
                {
                    //создаем галактику
                    new Galaxy("Orion", db.Sectors.FirstOrDefault(p => p.Id == 3), true);
                    Galaxy gs = db.Galaxys.FirstOrDefault(p => p.Name == "Orion");
                    //создаём карты
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "DES-666"), 0, 0, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-042"), 0, 1, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-358"), 0, 2, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-123"), 1, 0, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "NEU-007"), 1, 1, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-532"), 1, 2, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-752"), 2, 0, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "ORI-729"), 2, 1, true);
                    new Map(gs, db.Sectors.FirstOrDefault(p => p.Name == "EXP-888"), 2, 2, true);
                    //

                    db.SaveChanges();
                }

                if (db.Items.Count() == 0)
                {
                    db.Items.Add(new Item("Molecular substance", "This is what you get if you destroy something to its core", Rarity.Common, 1, 0));
                    db.Items.Add(new Item("Asteroid piece", "A small piece of cosmic junk", Rarity.Common, 1, 1));
                    db.Items.Add(new Item("Small rock", "Even smaller piece of cosmic junk. Totally useless", Rarity.Common, 1, 0));
                    db.Items.Add(new Item("Smooth rock", "A small piece of cosmic junk. Now polished", Rarity.Common, 2, 0));
                    db.Items.Add(new Item("Broken tech", "Maybe someone will find a better use for it", Rarity.Uncommon, 2, 2));
                    db.Items.Add(new Item("Melted fragment", "Now no one will ever figure out what it was originally", Rarity.Common, 1, 0));
                    db.Items.Add(new Item("Malfunctioned tech", "Looks less broken, but still doesn't work right", Rarity.Uncommon, 3, 0));
                    db.Items.Add(new Item("Working tech", "Surprisingly, it works!", Rarity.Rare, 4 , 4));
                    db.Items.Add(new Item("Scrap", "Misshapen pieces of unknown purpose", Rarity.Uncommon, 2, 0));
                    db.Items.Add(new Item("Shiny Working tech", "Now it just looks more shiny. What else did you expect?", Rarity.Rare, 5, 0));
                    db.SaveChanges();
                }

                if (db.Quests.Count() == 0)
                {
                    QuestGroup qgr1 = new QuestGroup();
                    db.Quests.Add(new Quest(qgr1, "What you can find in open space", "A space enthusiast is willing to pay you for asteroid samples. You sure have a few, don't you?", 0, 13, Tile.Model.Quest1));
                    db.QuestRequirements.Add(new QuestRequirement(qgr1, db.Items.FirstOrDefault(p => p.Name == "Asteroid piece"), 5));

                    QuestGroup qgr2 = new QuestGroup();
                    db.Quests.Add(new Quest(qgr2, "I can fix it!", "You stumble upon a mobile repairing station. For some reason it operates for free. Why not take a chance and fix some of your broken techs?", 0, 0, Tile.Model.Quest2));
                    db.QuestRequirements.Add(new QuestRequirement(qgr2, db.Items.FirstOrDefault(p => p.Name == "Broken tech"), 1));
                    db.QuestRewards.Add(new QuestReward(qgr2, db.Items.FirstOrDefault(p => p.Name == "Working tech"), 1));
                    db.SaveChanges();
                }

                if (db.Recepts.Count() == 0) 
                {
                    //Группа рецептов #1
                    ReceptGroup gr1 = new ReceptGroup("Astral");
                    db.Recepts.Add(new Recept(gr1, db.Items.FirstOrDefault(p => p.Name == "Molecular substance"), 1));
                    db.Recepts.Add(new Recept(gr1, db.Items.FirstOrDefault(p => p.Name == "Asteroid piece"), 1));
                    db.Recepts.Add(new Recept(gr1, db.Items.FirstOrDefault(p => p.Name == "Smooth rock"), 1));
                    //
                    db.Rewards.Add(new ReceptReward(gr1, db.Items.FirstOrDefault(p => p.Name == "Broken tech"), 5));

                    //Группа рецептов #2
                    ReceptGroup gr2 = new ReceptGroup("Valhalla");
                    db.Recepts.Add(new Recept(gr2, db.Items.FirstOrDefault(p => p.Name == "Scrap"), 3));
                    db.Recepts.Add(new Recept(gr2, db.Items.FirstOrDefault(p => p.Name == "Shiny Working tech"), 1));
                    //
                    db.Rewards.Add(new ReceptReward(gr2, db.Items.FirstOrDefault(p => p.Name == "Working tech"), 5));
                    db.Rewards.Add(new ReceptReward(gr2, db.Items.FirstOrDefault(p => p.Name == "Molecular substance"), 5));
                }

                if (db.LootBoxes.Count() == 0)
                {
                    List<LootBox> listItems = new List<LootBox>();
                    listItems.Add(new LootBox(db.Items.FirstOrDefault(s => s.Name == "Molecular substance"), 3000, 1));
                    listItems.Add(new LootBox(db.Items.FirstOrDefault(s => s.Name == "Asteroid piece"), 7000, 1));
                    LootBox.Create(listItems);
                }

                if (db.Monsters.Count() == 0)
                {
                    db.Monsters.Add(new Monster (db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Monster), 1, "Space worm", "Widespread space scavenger. Ingests everything that appears on its way.", db.LootBoxes.FirstOrDefault(s => s.Id == 1)));
                    db.SaveChanges();
                }
            }
        }
    }
}