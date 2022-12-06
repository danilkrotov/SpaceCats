using SpaceCatServer.Class.Enums;
using SpaceCatServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Fraction
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public Fraction(string name)
        {
            Name = name;
        }

        private Fraction() { }

        /// <summary>
        /// Возвращает фракцию по умолчанию
        /// </summary>
        public static Fraction GetDefaultFraction()
        {
            using (var db = new DataBaseContext())
            {
                Fraction fr = db.Fractions.FirstOrDefault(p => p.Name == Enum.GetName(Fractions.Neutral));
                if (fr == null)
                {
                    throw new Exception("Исключение: Не найдена фракция по умолчанию");
                }
                else
                {
                    return fr;
                }
            }
        }
    }
}
