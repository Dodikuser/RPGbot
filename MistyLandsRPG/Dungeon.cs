using Telegram.Bot;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;

namespace MistyLandsRPG
{
    internal class Dungeon
    {
        public string Name;
        public string Location;
        public string Description;       

        public async Task LoadDundeon(string name)
        {
            string query = "SELECT * FROM landsrpg.dungeons WHERE Name = @name";

            try
            {
                using (var cmd = new MySqlCommand(query, Program.connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Name = name;
                            Location = reader["Location_Name"].ToString();
                            Description = reader["Description"].ToString();
                        }
                        else { Console.WriteLine($"Получино имя данжона({name}) котогого нет в бд"); }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task Print(Update update)
        {
            if (Name == null) return;

            await Program.botClient.SendTextMessageAsync(
            chatId: MainCommands.GetChatId(update),
                                     text: $"Це підземелля: {Name} \nОпис: {Description}"
                                 );
        }
    }
}
