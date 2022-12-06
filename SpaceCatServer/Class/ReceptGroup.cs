using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class ReceptGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }

        internal ReceptGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Пустой конструктор для создания полей в БД
        /// </summary>
        public ReceptGroup() { }
    }
}
