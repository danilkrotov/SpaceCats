using Discord;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using SpaceCatServer.Class.MiniClass;
using SpaceCatServer.Database;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;

namespace SpaceCatServer.Class
{
    internal class MapFog
    {
        public int Id { get; private set; }
        public Account Account { get; private set; }
        public Map Map { get; private set; }
        /// <summary>
        /// Неуязвимая карта? не будет очищена из бд таблицы секторов при перезапуске мира
        /// </summary>
        public bool IsInvulnerable { get; private set; }
        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        private MapFog() { }
        internal MapFog(Account account, Map map, bool isInvulnerable = false)
        {
            Account = account;
            Map = map;
            IsInvulnerable = isInvulnerable;
            Save();
        }

        private void Save()
        {
            using (var db = new DataBaseContext())
            {
                this.Account = db.Accounts.FirstOrDefault(p => p.Id == this.Account.Id); //пробрасываем ссылку на Accounts
                this.Map = db.Maps.FirstOrDefault(p => p.Id == this.Map.Id); //пробрасываем ссылку на Maps
                db.MapFogs.Add(this);
                db.SaveChanges();
            }
        }

        public static MapFog? GetMapFog(string accountDid, int mapId) 
        {
            using (var db = new DataBaseContext())
            {
                MapFog? mapFog = db.MapFogs.Include(z => z.Account).Include(z => z.Map).Include(z => z.Map.Sector).FirstOrDefault(s => s.Account.Did == accountDid && s.Map.Id == mapId);
                return mapFog;
            }
        }
        /// <summary>
        /// Вовзращает список карт у двух аккаунтов в определенной галактике
        /// </summary>
        public static List<Map> DistinctTwoFogs(string discordid1, string discordid2, Galaxy gal) 
        {
            using (var db = new DataBaseContext())
            {
                List<Map> listMapFog = new List<Map>();
                foreach (Map mapFog in db.MapFogs.AsQueryable().Include(z => z.Account).Include(z => z.Map).Include(z => z.Map.Galaxy).Where(s => s.Account.Did == discordid1 || s.Account.Did == discordid2 || s.Map.Galaxy.Id == gal.Id).Select(s => s.Map).Distinct().ToList())
                {
                    listMapFog.Add(mapFog);
                }

                return listMapFog;
            }
        }
        /// <summary>
        /// Удаляет у аккаунта все известные сектора в галактике
        /// </summary>
        public static void DeleteInGalaxy(Account acc, Galaxy gal)
        {
            using (var db = new DataBaseContext())
            {
                foreach (MapFog mapFog in db.MapFogs.AsQueryable().Include(z => z.Account).Include(z => z.Map).Include(z => z.Map.Galaxy).Where(s => s.Account.Did == acc.Did && s.Map.Galaxy.Id == gal.Id))
                {
                    db.MapFogs.Remove(mapFog);
                }
                db.SaveChanges();
            }
        }
    }
}
