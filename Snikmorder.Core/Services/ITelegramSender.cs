using System.Threading.Tasks;
using Snikmorder.Core.Models;

namespace Snikmorder.Core.Services
{
    public interface ITelegramSender
    {
        Task SendMessage(Player player, string message) => SendMessage(player.TelegramUserId, message);
        Task SendMessage(int id, string message);
        Task SendImage(int id, string message, string? pictureId);

        Task SendImage(Player player, string message, string? pictureId) => SendImage(player.TelegramUserId, message, pictureId);
    }
}