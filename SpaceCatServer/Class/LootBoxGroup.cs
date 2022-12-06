using SpaceCatServer.Class;
using SpaceCatServer.Database;
using SpaceCatServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class LootBoxGroup
    {
        public int Id { get; set; }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        public LootBoxGroup() { }
    }
}
