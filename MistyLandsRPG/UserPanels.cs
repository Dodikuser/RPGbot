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
        static public async Task MenuPanel(Update update, Player player)
        {           
            // Количество кнопок в строке
            int buttonsPerRow = 3;
            var buttons = player.State.Commands.Keys.Select(key => new KeyboardButton(key)).ToList();

            var buttonRows = new List<KeyboardButton[]>();
            for (int i = 0; i < buttons.Count; i += buttonsPerRow)
            {
                buttonRows.Add(buttons
                    .Skip(i)
                    .Take(buttonsPerRow)
                    .ToArray());
            }
            
            var replyKeyboard = new ReplyKeyboardMarkup(buttonRows)
            {
                ResizeKeyboard = true,
            };

            await Program.botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "📝",
                replyMarkup: replyKeyboard);

            return;
        }
        
    }
}
