using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class Tile
    {
        [Key]
        public Model ModelId { get; set; }
        public Type Types { get; set; }

        public enum Model
        {
            NeutralCapital = 0,
            GoodCapital = 1,
            EvylCapital = 2,
            Wall = 3,
            Gate = 4,
            Hidden = 5,
            Empty = 6,
            Quest1 = 7,
            Monster = 8,
            Planet = 9,
            Asteroid = 10,
            Quest2 = 11
        }
        public enum Type
        {
            Empty = 1,
            City = 2,
            Monster = 3,
            Quest = 4,
            Gate = 5
        }
        public Tile(Model model, Type types)
        {
            ModelId = model;
            Types = types;
        }

        public Tile() { }
    }
}
