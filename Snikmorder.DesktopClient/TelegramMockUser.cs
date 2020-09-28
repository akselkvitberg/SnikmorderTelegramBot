using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Snikmorder.DesktopClient.Annotations;
using Telegram.Bot.Types;

namespace Snikmorder.DesktopClient
{
    public class TelegramMockUser : INotifyPropertyChanged
    {
        private readonly TelegramMockClient _client;

        public TelegramMockUser(int userId, string name, TelegramMockClient client)
        {
            _client = client;
            Name = name;
            UserId = userId;
        }

        public int UserId { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<TelegramMockMessage> Messages { get; } = new ObservableCollection<TelegramMockMessage>();

        private string _inputText;

        public string InputText
        {
            get { return _inputText; }
            set
            {
                if (value != _inputText)
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AddMessage(string message)
        {
            Messages.Add(new TelegramMockMessage(message, false));
        }

        #region SendMessageCommand

        private RelayCommand _sendMessageCommand;

        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(OnExecuteSendMessageCommand, OnCanExecuteSendMessageCommand)); }
        }

        private void OnExecuteSendMessageCommand(object o)
        {
            Messages.Add(new TelegramMockMessage(InputText, true));
            Messages.Add(new TelegramMockMessage(InputText, false));
            _client.SendMessage(UserId, InputText);
            InputText = "";
        }

        private bool OnCanExecuteSendMessageCommand(object o)
        {
            return !string.IsNullOrWhiteSpace(InputText);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TelegramMockMessage 
    {
        public string Message { get; }
        public bool Mine { get; }

        public TelegramMockMessage(string message, bool mine)
        {
            Message = message;
            Mine = mine;
        }
        
    }

    public class TelegramMockClient
    {
        private readonly HttpClient _client;
        private const string Uri = "https://localhost:10501/Message";

        public TelegramMockClient(HttpClient client)
        {
            _client = client;
        }

        public async Task SendMessage(int userId, string message)
        {
            var response = await _client.PostAsync(Uri, BuildMessage(userId, message));
            if (response.IsSuccessStatusCode)
            {
                
            }
            else
            {
                
            }
        }

        private HttpContent BuildMessage(in int userId, string text = null, string imagePath = null)
        {
            var msg = new Message()
            {
                From = new User()
                {
                    Id = userId
                },
                Text = text,
                Photo = new []
                {
                    new PhotoSize()
                    {
                        Height = 10,
                        Width = 10,
                        FileId = imagePath,
                    },
                }
            };

            return new StringContent(System.Text.Json.JsonSerializer.Serialize(msg), Encoding.UTF8, MediaTypeNames.Application.Json);
        }
    }
}