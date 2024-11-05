using Telegram.Bot;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;


namespace MistyLandsRPG
{
    internal class MainCommands
    {
        static public long GetChatId(Update update)
        {
            long chatId = 0;

            if (update.Type == UpdateType.Message && update.Message != null)
            {
                chatId = update.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Message != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.EditedMessage && update.EditedMessage != null)
            {
                chatId = update.EditedMessage.Chat.Id;
            }

            return chatId;
        }

        static async Task HandleInline(Message message) // пример кнопок
        {
            // Тут создаем нашу клавиатуру
            var inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("Это кнопка с сайтом", "https://habr.com/"),
                                            InlineKeyboardButton.WithCallbackData("А это просто кнопка", "button1"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Тут еще одна", "button2"),
                                            InlineKeyboardButton.WithCallbackData("И здесь", "button3"),
                                        },
                });

            await Program.botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Это inline клавиатура!",
                replyMarkup: inlineKeyboard); // Все клавиатуры передаются в параметр replyMarkup

            return;
        }
        static async Task HandReplyTest(Message message)// пример кногпок в клаве
        {
            // Тут все аналогично Inline клавиатуре, только меняются классы
            // НО! Тут потребуется дополнительно указать один параметр, чтобы
            // клавиатура выглядела нормально, а не как абы что

            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Привет!"),
                                            new KeyboardButton("Пока!"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Позвони мне!")
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Напиши моему соседу!")
                                        }
                })
            {
                // автоматическое изменение размера клавиатуры, если не стоит true,
                // тогда клавиатура растягивается чуть ли не до луны,
                // проверить можете сами
                ResizeKeyboard = true,
            };

            await Program.botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Это repy клавиатура!",
                replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup

            return;
        }

        static public async Task RegisterEx(Update update, Player player)
        {
            char[] forbiddenChars = { '/', '"', '\'', '\t', '\n', '\r' };

            if (update.Message.Text.IndexOfAny(forbiddenChars) != -1)
            {
                await Program.botClient.SendTextMessageAsync(
                                     chatId: update.Message.Chat.Id,
                                     text: "Таке ім'я не годится"
                                 );
            }
            else
            {
                player.Name = update.Message.Text;
                player.State = StatesContainer.States["menu"];
                player.Location = "Drybone";
                await player.UpdateData();

                await Program.botClient.SendTextMessageAsync(
                                     chatId: update.Message.Chat.Id,
                                     text: "Добре, теперь твоє ім'я " + player.Name
                                 );
                await UserPanels.GoToMenu(update, player);
            }
        }

        // основные исключения
        static public async Task NoCommands(Update update, Player player)
        {
            await Program.botClient.SendTextMessageAsync(
                                    chatId: GetChatId(update),
                                    text: "Не розумію тебе"
                                );
        }
        static public async Task PniRazraba(Update update, Player player)
        {
            await Program.botClient.SendTextMessageAsync(
                                    chatId: update.Message.Chat.Id,
                                    text: "Пни разраба, тут нет функционала"
                                );
        }

        // команды связаные с локациями
        
        static public async Task InstallMap(Update update, Player player)
        {
            player.State = StatesContainer.States["map"];
            await player.UpdateData();

            string imagePath = @"Images/Map_v1.jpg";
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                await Program.botClient.SendPhotoAsync(
                    chatId: update.Message.Chat.Id,
                    photo: InputFile.FromStream(fs)
                );
            }
            // запрос на получения всех локаций из бд
            List<string> locations = Location.GetAvailableLocations(player.Location);
            // вывожу кнопки со всеми локациями
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

            for (int i = 0; i < locations.Count; i += 4)
            {
                var row = new List<InlineKeyboardButton>();
                for (int j = 0; j < 4 && i + j < locations.Count; j++)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(locations[i + j], locations[i + j]));
                }
                buttons.Add(row.ToArray());
            }
            var inlineKeyboard = new InlineKeyboardMarkup(buttons);

            await Program.botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                $"Ви знаходетесь у місті {player.Location}. Це міста в які ви можете перейти:",
                replyMarkup: inlineKeyboard);
        }
        static public async Task GoToLocation(Update update, Player player)
        {
            if (update.Type != UpdateType.CallbackQuery) return;

            string targetLocation = update.CallbackQuery.Data.TrimEnd();

            List<string> locations = Location.GetAvailableLocations(player.Location);

            bool locationExist = locations.Contains(targetLocation);

            if (locationExist)
            {
                player.Location = targetLocation;
                await player.UpdateData();

                await Program.botClient.SendTextMessageAsync(
                                    chatId: GetChatId(update),
                                    text: $"Ви прибули у місто {player.Location}"
                                );
            }
            else
            {
                await Program.botClient.SendTextMessageAsync(
                                    chatId: GetChatId(update),
                                    text: $"Немає дороги з {player.Location} до міста {targetLocation}"
                                );
            }
        }
        

       
        // команды для нпс, возможно перенесу в отдельный класс
        static public async Task GoToNpcTest(Update update, Player player)
        {
            player.State = StatesContainer.States["npcTest"];
            await player.UpdateData();
        }
        static public async Task NpcTest(Update update, Player player)
        {
            NPC npc = new NPC();
            await npc.LoadNpc(update.Message.Text);
            await npc.Print(update);
        }

    }
}
