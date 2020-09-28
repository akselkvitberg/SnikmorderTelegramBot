using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public interface ITelegramSender
    {
        void SendMessage(Player player, string message);
    }
}