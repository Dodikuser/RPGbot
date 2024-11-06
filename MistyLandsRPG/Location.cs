using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace MistyLandsRPG
{
    internal class Location
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        List<string> Npc = new List<string>();
        List<string> Dungeons = new List<string>();
        List<string> AvailableLocations = new List<string>();
        /// <summary>
        /// Загрузка локации, убедись что локация есть в бд
        /// </summary>
        /// <param name="name">Имя локации</param>
        /// <returns></returns>
        public async Task LoadLocation(string name)
        {
            Name = name;
            Npc.Clear();
            Dungeons.Clear();
            AvailableLocations.Clear();

            AvailableLocations = GetAvailableLocations(name);

            string query = @"SELECT `location_id` 
                     FROM `landsrpg`.`locations` 
                     WHERE `Name` = @name;";
            using (var cmd = new MySqlCommand(query, Program.connection))
            {
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        Id = Convert.ToInt32(reader["location_id"]);
                    }
                }
            }

            string npsQuery = @"SELECT `Name` 
                     FROM `landsrpg`.`npc` 
                     WHERE `location` = @name;";
            using (var cmd = new MySqlCommand(npsQuery, Program.connection))
            {
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        Npc.Add(reader["Name"].ToString());
                    }
                }
            }

            string dungeonsQuery = @"SELECT `Name` 
                     FROM `landsrpg`.`dungeons` 
                     WHERE `Location_Name` = @name;";
            using (var cmd = new MySqlCommand(dungeonsQuery, Program.connection))
            {
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        Dungeons.Add(reader["Name"].ToString());
                    }
                }
            }
        }

        public override string ToString()
        {
            var dungeonsBuilder = new StringBuilder();
            foreach (var d in Dungeons)
            {
                dungeonsBuilder.Append(d).Append("\t");
            }

            var npcBuilder = new StringBuilder();
            foreach (var n in Npc)
            {
                npcBuilder.Append(n).Append("\t");
            }

            return $"Id: {Id}\tName: {Name}\tDungeons: {dungeonsBuilder}\tNpcs: {npcBuilder}";
        }

        static public List<string> GetAvailableLocations(string playerLocation)
        {
            List<string> locations = new List<string>();

            string query = "SELECT second_location FROM landsrpg.transitions WHERE first_location = @playerLocation";
            MySqlCommand cmd = new MySqlCommand(query, Program.connection);
            cmd.Parameters.AddWithValue("@playerLocation", playerLocation);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    locations.Add(reader["second_location"].ToString());
                }
            }

            return locations;
        }

        static public async Task ShowNpcs(Update update, Player player)
        {
            List<string> npcs = GetNpcOnLocation(player.Location);
            
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < npcs.Count; i += 4)
            {
                var row = new List<InlineKeyboardButton>();
                for (int j = 0; j < 4 && i + j < npcs.Count; j++)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(npcs[i + j], "0" + npcs[i + j]));
                }
                buttons.Add(row.ToArray());
            }
            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            string massage = npcs.Count != 0 ? $"У місті {player.Location} ви бачете перед собою деяких людей:" 
                : "У цьому місті ви не бачете нікого";
            await Program.botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                massage,
                replyMarkup: inlineKeyboard);


        }
        static public List<string> GetNpcOnLocation(string location)
        {
            List<string> npcs = new List<string>();

            string query = "SELECT Name FROM landsrpg.npc WHERE Location = @playerLocation";
            MySqlCommand cmd = new MySqlCommand(query, Program.connection);
            cmd.Parameters.AddWithValue("@playerLocation", location);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    npcs.Add(reader["Name"].ToString());
                }
            }

            return npcs;
        }
    }
}
