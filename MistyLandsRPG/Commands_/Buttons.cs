using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;

namespace MistyLandsRPG
{
    internal class Buttons
    {
        static public Dictionary<string, Func<Message, CallbackQuery, Task>> 
            buttonsHandlers = new Dictionary<string, Func<Message, CallbackQuery, Task>>
    {
        { "button1", TestButton1 },
        
    };

        static async Task TestButton1(Message message, CallbackQuery callbackQuery)
        {
            var chatId = message.Chat.Id;
            
            await Program.botClient.AnswerCallbackQueryAsync(callbackQuery.Id);          

            await Program.botClient.SendTextMessageAsync(
                chatId,
                $"Вы нажали на {callbackQuery.Data}");
            return;
        }
    }
}
