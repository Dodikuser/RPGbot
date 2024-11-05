using Telegram.Bot;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;

namespace MistyLandsRPG
{
    internal class AdminCommands
    {
        //команды для админа
        /// <summary>
        /// Панель для админа (кнопки в клавиатуре у пользователя)
        /// </summary>
        /// <param name="update"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        static public async Task AdminPanel(Update update, Player player)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Інформація про гравців"),
                                            new KeyboardButton("Показати гравців на локаціях"),
                                        },
                                        //new KeyboardButton[]
                                        //{
                                        //    new KeyboardButton("Квести"),
                                        //    new KeyboardButton("Бестіарій"),
                                        //},
                                        //new KeyboardButton[]
                                        //{
                                        //    new KeyboardButton("Друзі"),
                                        //    new KeyboardButton("Інформація про гравця")
                                        //}
                })
            {
                ResizeKeyboard = true,
            };
            await Program.botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Відкриваю панель адміна!",
                replyMarkup: replyKeyboard);

            return;
        }

        static public async Task ShowAllPlayer(Update update, Player player)
        {
            StringBuilder sb = new StringBuilder();

            string query = "SELECT Player_id, Name, Coins, location_now, state, role FROM players";
            using (MySqlCommand command = new MySqlCommand(query, Program.connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                sb.AppendLine("Список гравців:\n");
                sb.AppendLine("```\n" +
                              "ID   | Им'я           | Монети  | Локація     | Стан | Роль      \n" +
                              "-----|---------------|---------|-------------|-----------|-----------");

                while (reader.Read())
                {

                    string row = string.Format(
                        "{0,-4} | {1,-13} | {2,-7} | {3,-11} | {4,-9} | {5,-9}",
                        reader["Player_id"],
                        reader["Name"],
                        reader["Coins"],
                        reader["location_now"],
                        reader["state"],
                        reader["role"]);

                    sb.AppendLine(row);
                }
                sb.AppendLine("```");
            }

            string playersInfo = sb.ToString();
            await Program.botClient.SendTextMessageAsync(
                chatId: MainCommands.GetChatId(update),
                text: playersInfo,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
            );
        }

        static public async Task ShowPlayersOnLocations(Update update, Player player)
        {            
            string query = "SELECT Player_id, Name, location_now FROM players";
            
            Dictionary<string, List<string>> locationPlayers = new Dictionary<string, List<string>>();
            try
            {
                using (MySqlCommand command = new MySqlCommand(query, Program.connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {                       
                        long playerId = reader.GetInt64("Player_id");
                        string playerName = reader.GetString("Name");
                        string locationNow = reader.GetString("location_now");
                        
                        string playerInfo = $"{playerName}({playerId})";
                       
                        if (!locationPlayers.ContainsKey(locationNow))
                        {
                            locationPlayers[locationNow] = new List<string>();
                        }
                        locationPlayers[locationNow].Add(playerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            StringBuilder result = new StringBuilder();
            foreach (var location in locationPlayers)
            {
                result.AppendLine($"🏔 [{location.Key}]:"); 
                foreach (var playerInfo in location.Value)
                {
                    string s = new string(' ', location.Key.Length + 7);
                    result.AppendLine($"\t{s}{playerInfo}");
                }
            }
            
            await Program.botClient.SendTextMessageAsync(
                 chatId: MainCommands.GetChatId(update),
                 text: result.ToString()
                //parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
             );
        }

    }
}
