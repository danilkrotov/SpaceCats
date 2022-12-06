using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class GalaxyGeneration
    {
        private static List<Tile> sectorsList = new List<Tile>();
        internal static List<Tile> GenerateLevel1() 
        {
            using (var db = new DataBaseContext())
            {
                AddSector(db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Empty), 10);
                AddSector(db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Monster), 70);
                AddSector(db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Quest1), 10);
                AddSector(db.Tiles.FirstOrDefault(p => p.ModelId == Tile.Model.Quest2), 10);
                return sectorsList;
            }
        }

        /// <summary>
        /// В массиве должно быть 100 едениц секторов. Добавляем тип сектора и его количество в %
        /// </summary>
        private static void AddSector(Tile sectorType, int count) 
        {
            for (int i = 0; i < count; i++)
            {
                sectorsList.Add(sectorType);
            }            
        }
    }
}
