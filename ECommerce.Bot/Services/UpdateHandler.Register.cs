using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using ECommerce.Domain.Enums;

namespace ECommerce.Bot.Services;

public partial class UpdateHandler
{
    private async Task<Message> Start(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nameof(message.From));
        var userDb = await GetUserAsync(message);

        ReplyKeyboardMarkup replyKeyboardMarkup = new([
                ["🇷🇺 Русский", "🇺🇿 O'zbekcha"],
            ["🇬🇧 English"]
            ])
        {
            ResizeKeyboard = true
        };

        var msg = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            @"Здравствуйте! Давайте для начала выберем язык обслуживания!

Keling, avvaliga xizmat ko’rsatish tilini tanlab olaylik.

Hi! Let's first we choose language of serving!",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);

        return msg;
    }
    private async Task<Message> ChangeLanguage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var userDb = await GetUserAsync(message);
        var language = message.Text switch
        {
            "🇷🇺 Русский" => LanguageType.Russian,
            "🇬🇧 English" => LanguageType.English,
            _ => LanguageType.Uzbek
        };
        if (language != LanguageType.Uzbek)
            await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Kechirasiz, lekin hozircha faqat o'zbek tili mavjud!",
                    cancellationToken: cancellationToken);
        var msg = await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Fish and Breadda elektron hamyon ochish uchun quyidagi qo'llanmaga amal qiling!" +
                        "\n{Qo'llanma uchun havola}",
                        cancellationToken: cancellationToken);
        userDb.RegisterStep = UserRegisterStepType.Language;
        userDb.Language = language;
        _userRepository.Update(userDb);
        await _userRepository.SaveChangesAsync();
        if (userDb.RegisterStep <= UserRegisterStepType.Language)
        {
            msg = await SendShareContactButtonAsync(botClient, message, cancellationToken);
        }
        return msg;
    }
    private async Task<Message> ChangeFullNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var userDb = await GetUserAsync(message);
        if (userDb.RegisterStep >= UserRegisterStepType.Phone)
        {
            userDb.RegisterStep = UserRegisterStepType.FullName;
            userDb.FullName = message.Text;
            _userRepository.Update(userDb);
            await _userRepository.SaveChangesAsync();
        }
        return await SendMainMenuAsync(botClient, message, cancellationToken);
    }
}
