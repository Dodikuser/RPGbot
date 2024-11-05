using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MistyLandsRPG
{
   

    internal class Player
    {
        public long Id;
        public string Name;
        //Wepon, armor, charm, active quest
        public long Coins;
        public string Location;
        public State State;
        public string Role;

        public async Task ExecuteCommand(Update update, string command)
        {           
            if (State.Commands.ContainsKey(command))
            {
                await State.Commands[command].Invoke(update, this);
            }
            else
            {
                await ExecuteEx(update);
            }
        }
        public async Task ExecuteEx(Update update)
        {
            State?.ExclusionMethod?.Invoke(update, this);
        }

        public async Task UpdateData()
        {
            string query = @"UPDATE `landsrpg`.`players` 
                     SET `Name` = @Name, `Coins` = @Coins, `state` = @state, `location_now` = @location
                     WHERE `Player_id` = @userId;";

            using (var cmd = new MySqlCommand(query, Program.connection))
            {
                cmd.Parameters.AddRange(new[]
                {
            new MySqlParameter("@Name", Name),
            new MySqlParameter("@Coins", Coins),
            new MySqlParameter("@state", State.MyStateName),
            new MySqlParameter("@location", Location),
            new MySqlParameter("@userId", Id)
        });
                try
                {
                    await cmd.ExecuteNonQueryAsync();

                }catch (Exception ex) {Console.WriteLine(ex.ToString());}
            }
        }
        public async Task LoadPlayer(long id)
        {
            Id = id;
            string query = @"SELECT `Name`, `Coins`, `location_now`, `state`, `role` 
                     FROM `landsrpg`.`players` 
                     WHERE `Player_id` = @PlayerId;";

            using (var cmd = new MySqlCommand(query, Program.connection))
            {
                cmd.Parameters.AddWithValue("@PlayerId", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {                      
                        Name = reader["Name"] as string;
                        //Weapon = reader["Weapon"] as string;
                        //Armor = reader["Armor"] as string;
                        //Charm = reader["Charm"] as string;
                        //ActiveQuest = reader["Active_Quest"] as string;
                        Coins = reader["Coins"] != DBNull.Value ? (long)reader["Coins"] : 0;
                        Location = reader["location_now"] as string;
                        string stateName = reader["state"] as string;
                        State = (StatesContainer.States.ContainsKey(stateName))? StatesContainer.States[stateName] : null;
                        Role = reader["role"] as string;
                    }
                }
            }
        }

        static public async Task PlayerInfo(Update update, Player player)
        {
            await Program.botClient.SendTextMessageAsync(
                                    chatId: MainCommands.GetChatId(update),
                                    text: $"Інформація про гравця\n" +
                                    $"Ваше ім'я: {player.Name}\n" +
                                    $"Зараз ви на локації: {player.Location}\n" +
                                    $"Зараз ваш стан: {player.State.MyStateName}"
                                );
        }
    }
}
