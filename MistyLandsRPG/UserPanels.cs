using Telegram.Bot;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MySql.Data.MySqlClient;

namespace MistyLandsRPG
{
    internal class UserPanels
    {
        static public async Task GoToMenu(Update update, Player player)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Подивитися карту"),
                                            new KeyboardButton("Інвентар"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Квести"),
                                            new KeyboardButton("Бестіарій"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("Друзі"),
                                            new KeyboardButton("Інформація про гравця")
                                        }
                })
            {
                ResizeKeyboard = true,
            };
            await Program.botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Відкриваю меню!",
                replyMarkup: replyKeyboard);

            return;
        }
    }
}
