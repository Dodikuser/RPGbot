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
        public long Coins;
        public string Role;

        public State State;
        public string Location;
        public string TargetLocation;
        public string NpcTalking;
        public string Dungeon;
        public string Event;

        //Wepon, armor, charm, active quest
        
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
            string query = @"
        UPDATE landsrpg.players p
        JOIN landsrpg.players_states ps ON p.Player_id = ps.Player_id
        SET 
            p.Name = @Name, 
            p.Coins = @Coins, 
            p.role = @Role, 
            ps.state = @state, 
            ps.location_now = @location, 
            ps.npc_talking = @npcTalking, 
            ps.dungeon_now = @dungeon,
            ps.target_location = @targetLocation,
            ps.transition_event = @event
        WHERE 
            p.Player_id = @userId;";

            using (var cmd = new MySqlCommand(query, Program.connection))
            {
                cmd.Parameters.AddRange(new[]
                {
            new MySqlParameter("@Name", Name),
            new MySqlParameter("@Coins", Coins),
            new MySqlParameter("@Role", Role),
            new MySqlParameter("@state", State.MyStateName),
            new MySqlParameter("@location", Location),
            new MySqlParameter("@npcTalking", NpcTalking),
            new MySqlParameter("@dungeon", Dungeon),
            new MySqlParameter("@userId", Id),
            new MySqlParameter("@targetLocation", TargetLocation),
            new MySqlParameter("@event", Event),
        });

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task LoadPlayer(long id)
        {
            Id = id;
            string query = @"
                            SELECT 
                                 p.*, 
                                 ps.*
                            FROM 
                                landsrpg.players p
                            JOIN 
                                landsrpg.players_states ps 
                            ON 
                                p.Player_id = ps.Player_id
                            WHERE 
                                p.Player_id = @PlayerId;
                        ";

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
                        TargetLocation = reader["target_location"] as string;
                        Event = reader["transition_event"] as string;
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
