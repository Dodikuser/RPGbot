using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static MistyLandsRPG.MainCommands;

namespace MistyLandsRPG
{   
    internal class State
    {
        public Dictionary<string, Func<Update, Player, Task>> Commands;
        public Func<Update, Player, Task> ExclusionMethod;
        public Func<Update, Player, Task> OpenPanelMethod;
        public string MyStateName;

        public State
            (
            Dictionary<string, Func<Update, Player, Task>> commands, 
            Func<Update, Player, Task> exMethod,
            string stateName, Func<Update, Player, Task> panelMethod = null
            ) 
        {
            Commands = commands;
            ExclusionMethod = exMethod;
            MyStateName = stateName;
        }

    }

    internal class StatesContainer
    {
        static public Dictionary<string, State> States = new Dictionary<string, State>()
        {
            {"register", new State(new Dictionary<string, Func<Update, Player, Task>>(), RegisterEx, "register") },           
            {"menu", 
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){ 
                        {"Подивитися карту", InstallMap },
                        {"Інвентар", PniRazraba },
                        {"Квести", GoToNpcTest },
                        {"Бестіарій",PniRazraba },
                        {"Друзі", PniRazraba },
                        {"Інформація про гравця", Player.PlayerInfo },
                    }, NoCommands, "menu") },
            {"map",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту", InstallMap },
                        {"Інвентар", PniRazraba },
                        {"Квести", GoToNpcTest },
                        {"Бестіарій", PniRazraba },
                        {"Друзі", PniRazraba },
                        {"Інформація про гравця", Player.PlayerInfo },
                    },  GoToLocation, "map") },
            {"npcTest",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту", InstallMap },
                        {"Інвентар", PniRazraba },
                        {"Квести", GoToNpcTest },
                        {"Бестіарій", PniRazraba },
                        {"Друзі", PniRazraba },
                    }, NpcTest, "npcTest") },
            {"Admin",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Інформація про гравців", AdminCommands.ShowAllPlayer},
                        {"Показати гравців на локаціях", AdminCommands.ShowPlayersOnLocations},
                        
                    }, NoCommands, 
             "Admin") },
        };
    }
}
