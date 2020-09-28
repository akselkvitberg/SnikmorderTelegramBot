using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public interface ITelegramSender
    {
        void SendMessage(Player player, string message) => SendMessage(player.TelegramUserId, message);
        void SendMessage(int id, string message);
        void SendImage(int id, string message, string? pictureId);

        void SendImage(Player player, string message, string? pictureId) => SendImage(player.TelegramUserId, message, pictureId);
    }
}