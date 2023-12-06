using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using ECommerce.Domain.Enums;

namespace ECommerce.Bot.Services;

public partial class UpdateHandler
{
    private async Task BotOnContactReceived(ITelegramBotClient botClient, Message message, Contact contact, CancellationToken cancellationToken)
    {
        var userDb = await GetUserAsync(message);
        if (contact.UserId != message.From.Id)
        {
            await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Iltimos o'zingizning telefon raqamingizni yuboring!",
                            cancellationToken: cancellationToken);
            return;
        }
        if (userDb.RegisterStep <= Domain.Enums.UserRegisterStepType.Language)
        {
            userDb.RegisterStep = Domain.Enums.UserRegisterStepType.Phone;
        }
        userDb.Phone = contact.PhoneNumber;
        _userRepository.Update(userDb);
        await _userRepository.SaveChangesAsync();
        await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Rahmat! Sizning telefon raqamingiz qabul qilindi!",
                                replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: cancellationToken);
        if (userDb.RegisterStep < Domain.Enums.UserRegisterStepType.FullName)
            await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "Iltimos, ismingizni va familiyangizni kiriting!",
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    cancellationToken: cancellationToken);
    }

    private async Task<Message> SendContectInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
         return await botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: "Agar sizda savollar bo'lsa bizga telefon qilishingiz mumkin: +998 93-683-15-55",
                                    cancellationToken: cancellationToken);
    }

    private async Task<Message> SendShareContactButtonAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup RequestReplyKeyboard = new([
                    KeyboardButton.WithRequestContact("📱 Raqamni jo'natish"),
        ])
        {
            ResizeKeyboard = true
        };

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "📱 Telefon raqamingiz qanday? Telefon raqamingizni jo'natish uchun, quyidagi \"📱 Raqamni jo'natish\" tugmasini bosing.",
            replyMarkup: RequestReplyKeyboard,
            cancellationToken: cancellationToken);
    }


    private async Task<Message> SendCommentAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var userDb = await GetUserAsync(message);
        userDb.Step = UserStep.Comment;
        return await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "O'z fikr va mulohazalaringizni jo'nating.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
    }
    private async Task<Message> SendCommentTypeButtonsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new([
            ["Hammasi yoqdi ❤️"],
            ["Yaxshi ⭐️⭐️⭐️⭐️"],
            ["Yoqmadi ⭐️⭐️⭐️"],
            ["Yomon ⭐️⭐️"],
            ["Juda yomon ⭐️"],
            ["⬅️ Orqaga"]
        ]);
        return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Fish and Breadni tanlaganingiz uchun rahmat.\r\nAgar siz bizning xizmat sifatimizni yaxshilashimizga yordam bersangiz hursand bulardik.\r\nBuning uchun 5 bal tizim asosida baholang",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
    }
    private async Task<Message> SendMainMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new([
                ["🛒 Buyurtma berish"],
            ["📝 Fikr bildirish", "📞 Biz bilan aloqa"],
            ["ℹ️ Ma'lumot", "⚙️ Sozlamalar"]
            ])
        {
            ResizeKeyboard = true
        };
        return await botClient.SendTextMessageAsync(
                       message.Chat.Id,
                                  "Juda yaxshi birgalikda buyurtma beramizmi? 😃",
                                             replyMarkup: replyKeyboardMarkup,
                                                        cancellationToken: cancellationToken);
    }
}
