using Telegram.Bot;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;

namespace MistyLandsRPG
{
    internal class NPC
    {
        public string Name;
        public string Location;
        public string DefaultSpeech;

        public Dictionary<string, string> Speeches;

        public async Task LoadNpc(string name) 
        {
            string query = "SELECT * FROM landsrpg.npc WHERE Name = @name";

            using (var cmd = new MySqlCommand(query, Program.connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Name = name;
                        Location = reader["Location"].ToString();
                        DefaultSpeech = reader["Speech"].ToString();
                    }
                    else { Console.WriteLine($"Получино имя нпс({name}) котогого нет в бд"); }
                }
            }
        }

        public async Task Print(Update update)
        {
            if (Name == null) return;

            await Program.botClient.SendTextMessageAsync(
            chatId: MainCommands.GetChatId(update),
                                     text: $"Мене звуть {Name} \n {DefaultSpeech}"
                                 );
        }
    }
}
