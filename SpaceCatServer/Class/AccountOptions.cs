using SpaceCatServer.Class.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class
{
    internal class AccountOptions
    {
        public SecurityLevel SecurityLevel = SecurityLevel.User;
        public Locale Localization = Locale.En;
        public int StartEssence = 0;
    }
}
