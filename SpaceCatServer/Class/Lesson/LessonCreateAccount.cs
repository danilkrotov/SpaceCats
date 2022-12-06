using SpaceCatServer.Class.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class.Lesson
{
    internal class LessonCreateAccount : ILesson
    {
        public string HandleCommand(string[] args)
        {
            AccountOptions options = new AccountOptions();
            options.StartEssence = int.Parse(args[2]);
            options.SecurityLevel = (SecurityLevel)int.Parse(args[3]);
            new Account(args[1], options);
            //log.AddLog("Аккаунт [" + args[1] + "] успешно зарегестрирован", false);
            return Localization.Name("SucAccount");
        }
    }
}
