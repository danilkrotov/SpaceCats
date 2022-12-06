// See https://aka.ms/new-console-template for more information
using SpaceCatServer.Database;

Console.WriteLine("Start init");
DataBaseContext.AfterInit(); //database init
new SpaceCatServer.Class.Discord(new Discord.WebSocket.DiscordSocketClient(), "");
while (true);