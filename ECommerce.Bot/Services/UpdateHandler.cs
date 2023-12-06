using ECommerce.Data.IRepositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Entities.Users;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = ECommerce.Domain.Entities.Users.User;

namespace ECommerce.Bot.Services;

public partial class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserComment> _userCommentRepository;
    private readonly IRepository<Branch> _branchRepository;

    public UpdateHandler(ITelegramBotClient botClient,
        ILogger<UpdateHandler> logger,
        IRepository<User> userRepository,
        IRepository<UserComment> userCommentRepository,
        IRepository<Branch> branchRepository)
    {
        _botClient = botClient;
        _logger = logger;
        _userRepository = userRepository;
        _userCommentRepository = userCommentRepository;
        _branchRepository = branchRepository;
    }
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { Contact: { } contact } message } => BotOnContactReceived(_botClient, message, contact, cancellationToken),
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        }; ;

        await handler;
    }


    private async Task BotOnMessageReceived(Message? message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;
        var anyBranch = await _branchRepository.SelectAll().AnyAsync(b => b.Name.Equals(messageText));
        if (anyBranch)
        {
            await SendBranchInfoAsync(_botClient, message, cancellationToken);
            return;
        }
        var action = messageText switch
        {
            "/start" => Start(_botClient, message, cancellationToken),
            "⬅️ Orqaga" => SendMainMenuAsync(_botClient, message, cancellationToken),
            "🇷🇺 Русский" or "🇺🇿 O'zbekcha" or "🇬🇧 English" => ChangeLanguage(_botClient, message, cancellationToken),
            "📞 Biz bilan aloqa" => SendContectInfoAsync(_botClient, message, cancellationToken),
            "📝 Fikr bildirish" => SendCommentTypeButtonsAsync(_botClient, message, cancellationToken),
            "Hammasi yoqdi ❤️" or "Yaxshi ⭐️⭐️⭐️⭐️" or "Yoqmadi ⭐️⭐️⭐️" or "Yomon ⭐️⭐️" or "Juda yomon ⭐️" => SendCommentAsync(_botClient, message, cancellationToken),
            "ℹ️ Ma'lumot" => SendInfoAsync(_botClient, message, cancellationToken),
            "🛒 Buyurtma berish" => SendOrderTypeButtonsAsync(_botClient, message, cancellationToken),
            "🚖 Yetkazib berish" => SendOrderDeliveryAsync(_botClient, message, cancellationToken),
            "🏃‍ Olib ketish" => SendOrderPickupAsync(_botClient, message, cancellationToken),
            //"/inline_keyboard" => BotOnMessage.SendInlineKeyboard(_botClient, message, cancellationToken),
            //"/keyboard" => BotOnMessage.SendReplyKeyboard(_botClient, message, cancellationToken),
            //"/remove" => BotOnMessage.RemoveKeyboard(_botClient, message, cancellationToken),
            //"/photo" => BotOnMessage.SendFile(_botClient, message, cancellationToken),
            //"/request" => BotOnMessage.RequestContactAndLocation(_botClient, message, cancellationToken),
            //"/inline_mode" => BotOnMessage.StartInlineQuery(_botClient, message, cancellationToken),
            //"/throw" => BotOnMessage.FailingHandler(_botClient, message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

    }

    private async Task<Message> SendOrderPickupAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        List<KeyboardButton> btns = [KeyboardButton.WithRequestLocation("Eng yaqin filialni aniqlash")];
        var branches = _branchRepository.SelectAll().Select(b => b.Name).ToList();
        btns.AddRange(branches.Select(b => new KeyboardButton(b)));
        btns.Add(new KeyboardButton("⬅️ Orqaga"));
        ReplyKeyboardMarkup RequestReplyKeyboard = new(btns)
        {
            ResizeKeyboard = true
        };
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Siz qayerda joylashgansiz 👀?\r\nAgar lokatsiyangizni jo'natsangiz 📍, sizga yaqin bo'lgan filialni aniqlaymiz",
            replyMarkup: RequestReplyKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task<Message> SendOrderDeliveryAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup RequestReplyKeyboard = new([
                    KeyboardButton.WithRequestLocation("Eng yaqin filialni aniqlash"),
                    new KeyboardButton("⬅️ Orqaga")
        ])
        {
            ResizeKeyboard = true
        };
        return await botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                                  text: "Buyurtmangizni qayerga yetkazib berish kerak 🚙?",
                                             replyMarkup: RequestReplyKeyboard,
                                                        cancellationToken: cancellationToken);
    }

    private Task<Message> SendOrderTypeButtonsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new([
            ["🚖 Yetkazib berish", "🏃‍ Olib ketish"],
            ["⬅️ Orqaga"]
        ]);
        return botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                    text: "Buyurtmani o'zingiz olib keting, yoki Yetkazib berishni tanlang",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
    }

    private async Task<Message> SendBranchInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.SelectAsync(b => b.Name.Equals(message.Text));
        await _botClient.SendLocationAsync(
            chatId: message.Chat.Id,
            latitude: branch!.Lat,
            longitude: branch.Lng,
            cancellationToken: cancellationToken);
        return await botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                        text: branch!.Description,
                        cancellationToken: cancellationToken);
    }

    private async Task<Message> SendInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var branches = await _branchRepository.SelectAll().Select(b => b.Name).ToListAsync();
        branches.Add("⬅️ Orqaga");
        var btns = branches.Select(b => new List<KeyboardButton> { b });
        var replyMarkup = new ReplyKeyboardMarkup(btns)
        {
            ResizeKeyboard = true
        };
        return await botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                                  text: "Iltimos, filialni tanlang",
                                             replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
    }

    private async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var userDb = await GetUserAsync(message);
        if (userDb.RegisterStep <= UserRegisterStepType.Language)
            return await Start(botClient, message, cancellationToken);
        if (userDb.RegisterStep < UserRegisterStepType.Phone)
            return await SendShareContactButtonAsync(botClient, message, cancellationToken);
        if (userDb.RegisterStep < UserRegisterStepType.FullName)
            return await ChangeFullNameAsync(botClient, message, cancellationToken);
        if (userDb.Step == UserStep.Comment)
        {
            await _userCommentRepository.InsertAsync(new UserComment
            {
                UserId = userDb.Id,
                Message = message.Text,
                CommentType = userDb.CommentType ?? CommentType.None,
            });
            await _userCommentRepository.SaveChangesAsync();
            userDb.Step = null;
            userDb.CommentType = null;
            await _userRepository.SaveChangesAsync();
            await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Fikr va mulohazalaringiz uchun rahmat!",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
            return await SendMainMenuAsync(botClient, message, cancellationToken);
        }
        return null;
    }
    private Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    private Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
    private async Task<User> GetUserAsync(Message message)
    {
        ArgumentNullException.ThrowIfNull(nameof(message.From));
        var userDb = await _userRepository.SelectAsync(x => x.TelegramId == message.From.Id);
        if (userDb is null)
        {
            var user = new User
            {
                TelegramId = message.From.Id,
                RegisterStep = UserRegisterStepType.Start,
                Username = message.From.Username,
                FullName = $"{message.From.FirstName} {message.From.LastName}",
            };
            user = await _userRepository.InsertAsync(user);
            await _userRepository.SaveChangesAsync();
            return user;
        }
        return userDb;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
