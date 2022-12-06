using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCatServer.Class.Enums
{
    public enum Locale
    {
        Ru = 0,
        En = 1
    }
    
    internal class Localization
    {
        /// <summary>
        /// Задаём локализацию для указанной сессии, по умолчанию En
        /// </summary>
        public static Locale Locale = Locale.En;
        public static string Name(string param)
        {
            ResourceManager? rm = null;
            if (Locale == Locale.Ru) { rm = Resources.Lang.Ru.ResourceManager; }
            if (Locale == Locale.En) { rm = Resources.Lang.En.ResourceManager; }

            if (rm == null)
            {
                throw new Exception("Выполнялся поиск локализации: " + Enum.GetNames(Locale.GetType()) + ", данная локализация не была найдена" );
            }

            string? answer = rm.GetObject(param).ToString();
            if (String.IsNullOrEmpty(answer)) 
            {
                throw new Exception("Была запрошена локализация переменной: " + param + ", локализация на языке " + Enum.GetNames(Locale.GetType()) + " не найдена");
            }
            return answer;
        }
    }
}
