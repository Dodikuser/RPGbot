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
        public string MyStateName;

        public State
            (
            Dictionary<string, Func<Update, Player, Task>> commands, 
            Func<Update, Player, Task> exMethod,
            string stateName
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
                        {"Квести", PniRazraba },
                        {"Бестіарій",PniRazraba },
                        {"Друзі", PniRazraba },
                        {"Інформація про гравця", Player.PlayerInfo },
                        {"Повернутися в основне меню", GoToBasicMenu },
                    }, NoCommands, "menu") },
            {"map",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту", InstallMap },
                        {"Повернутися в основне меню", GoToBasicMenu },                       
                    },  GoToLocation, "map")},
            {"inventory",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){                        
                        {"Повернутися в основне меню", GoToBasicMenu },
                    },  GoToLocation, "map")},
            {"basic_menu",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту", InstallMap },
                        {"Інвентар", PniRazraba },                                             
                        {"Відкрити меню локації", GoToLocationMenu },
                    }, NoCommands, "basic_menu") },
            {"location_menu",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту", InstallMap },
                        {"Переглянути Підземелля", Location.ShowDungeons },
                        {"Переглянути НПС",Location.ShowNpcs },
                        {"Перейти в меню", GoToMenu },
                    },  ProcessingButtonsForLocationMenu, "location_menu")},
            {"dungeon",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Подивитися карту підземелля", PniRazraba },                       
                        {"Сдатися(зрада)", GoToLocationMenu },                       
                    },  NoCommands, "dungeon")},
            {"fight",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Удар", PniRazraba },
                        {"Блок", PniRazraba },
                        {"Сдатися(зрада)", PniRazraba },
                    },  NoCommands, "fight")},
            {"npc",
                new State(
                    new Dictionary<string, Func<Update, Player, Task>>(){
                        {"Повернутися до меню локації", GoToLocationMenu },
                        {"Поговорити", PniRazraba },
                    },  NoCommands, "npc")},
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
