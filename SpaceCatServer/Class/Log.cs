using SpaceCatServer.Database;

namespace SpaceCatServer.Class
{
    internal class Log
    {
        public int ID { get; private set; }
        public string Did { get; private set; }
        public string Command { get; private set; }
        public bool Error { get; private set; }
        public string ErrorMessage { get; private set; }
        internal Log(string command, string did)
        {
            Command = command;
            ErrorMessage = "";
            Did = did;
        }
        /// <summary>
        /// Добавить данные в лог, по умолчанию флаг - Ошибка
        /// </summary>
        public void AddLog(string errorMessage, bool error = true) 
        {
            Error = error;
            ErrorMessage = errorMessage;
            Console.WriteLine("[" +this.Did + "] " +errorMessage); //Отображать всё что логируется в консоль
        }
        /// <summary>
        /// Сохранить в БД
        /// </summary>
        public void Save()
        {
            using (var db = new DataBaseContext())
            {
                db.Logs.Add(this);
                db.SaveChanges();
            }
        }
    }
}
