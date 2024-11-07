using Telegram.Bot.Types;
using static MistyLandsRPG.MainCommands;

namespace MistyLandsRPG
{
    internal class Event
    {
        string Name;
        public Func<Update, Player, Task> ActionMethod;

        public Event(string name, Func<Update, Player, Task> method )
        {
            Name = name;
            ActionMethod = method;
        }

    }

    internal class EventContainer
    {
        static public async Task Roberrs(Update update, Player player)
        {
            string massage = "Мы тебя ГраБаНем";
            Console.WriteLine(massage);           
        }

        static public Dictionary<string, Event> Events = new Dictionary<string, Event>()
        {
            {"roberrs", new Event("roberrs", Roberrs) }
        };
    }
}
