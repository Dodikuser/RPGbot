using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using MySql.Data.MySqlClient;


namespace MistyLandsRPG
{
    class Program
    {
        static public ITelegramBotClient botClient;
        static public MySqlConnection connection;

        static async Task Main(string[] args)
        {
            await CreateConnectionBD();

            string token = "7320793799:AAGtayCsiPDrwgQAX_1F8y0zxTsnnka_drM";
            botClient = new TelegramBotClient(token);

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {me.Username} запущен.");

            // Ожидаем завершения
            Console.ReadLine();
            cts.Cancel();
        }
        private static async Task CreateConnectionBD()
        {
            string connectionString = "Server=localhost;Database=landsrpg;User ID=root;Password=75259;";
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                Console.WriteLine("Соединение с базой данных успешно установлено.");

                string query = "SELECT * FROM locations";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader["Name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }

        }

        private static async Task RegisterUser(long userId, ITelegramBotClient botClient, long chatId)
        {
            string query = "INSERT INTO `landsrpg`.`players` (`Player_id`) VALUES (@userId); " +
                           "INSERT INTO `landsrpg`.`players_states` (`Player_id`, `state`) VALUES (@userId, 'register');";            

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            };
            await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Доброго дня, ви зареєстровані. Дайте ім'я своєму персонажу."
                                );
        }

        private static bool UserExists(long userId)
        {
            string query = "SELECT COUNT(*) FROM `landsrpg`.`players` WHERE `Player_id` = @userId";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetInt32(0) > 0;
                }
            }
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            if (update.Message is not { } message)
                                return;
                            if (message.Text is not { } messageText)
                                return;

                            long userId = update.Message.From.Id;
                            var chatId = message.Chat.Id;

                            if (!UserExists(userId))
                            {
                                await RegisterUser(userId, botClient, chatId);
                                return;
                            }                          

                            Console.WriteLine($"Получено сообщение '{messageText}' в чате {chatId}.");                            
                            string command = messageText.TrimEnd();

                            
                            Player player = new Player();
                            await player.LoadPlayer(userId);                           

                            player?.ExecuteCommand(update, command);
                            
                            return;
                        }
                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery;
                            var user = callbackQuery.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");

                            var chat = callbackQuery.Message.Chat;
                            string command = callbackQuery.Data.TrimEnd();
                            long userId = callbackQuery.From.Id;

                            Player player = new Player();
                            await player.LoadPlayer(userId);
                            player?.ExecuteCommand(update, command);

                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        // Обработка ошибок
        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            return Task.CompletedTask;
        }

    }
}
