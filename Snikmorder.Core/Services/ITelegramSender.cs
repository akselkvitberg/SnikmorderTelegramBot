using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public interface ITelegramSender
    {
        void SendMessage(Player player, string message);
        void SendMessage(int id, string message);
        void SendImage(int id, string message, string? pictureId);
    }
}