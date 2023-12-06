using ECommerce.Bot.Abstract;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using Telegram.Bot;

namespace ECommerce.Bot.Services;

// Compose Receiver and UpdateHandler implementation
public class ReceiverService : ReceiverServiceBase<UpdateHandler>
{
    public ReceiverService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        ILogger<ReceiverServiceBase<UpdateHandler>> logger)
        : base(botClient, updateHandler, logger)
    {
    }
}
