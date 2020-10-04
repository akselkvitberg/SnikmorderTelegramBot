using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Snikmorder.DesktopClient.Annotations;
using Snikmorder.DesktopClient.Utilities;

namespace Snikmorder.DesktopClient.GameMock
{
    public class TelegramMockUser : INotifyPropertyChanged
    {
        public TelegramMockUser(int userId, GameHostService gameHostService, bool isAdmin)
        {
            _gameHostService = gameHostService;
            UserId = userId;
            IsAdmin = isAdmin;
            InputText = IsAdmin ? "/neste" : "";

            if(!isAdmin)
                OnExecuteMakePlayerCommand(null);
            else
            {
                HasRunMakePlayerCommand = true; // disable button for admin - does nothing
            }
        }

        public int UserId { get; set; }
        public bool IsAdmin { get; }
        public string AdminText => IsAdmin ? "Adm" : "";


        public ObservableCollection<TelegramMockMessage> Messages { get; } = new ObservableCollection<TelegramMockMessage>();

        private string _inputText = "";

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

        public void AddImage(string message, string imagePath)
        {
            Messages.Add(new TelegramMockMessage(message, false, imagePath));
        }

        #region SendMessageCommand

        private RelayCommand _sendMessageCommand;
        private readonly GameHostService _gameHostService;

        public RelayCommand SendMessageCommand => _sendMessageCommand ??= new RelayCommand(OnExecuteSendMessageCommand, OnCanExecuteSendMessageCommand);

        private void OnExecuteSendMessageCommand(object o)
        {
            Messages.Add(new TelegramMockMessage(InputText, true));
            _gameHostService.SendMessage(UserId, InputText);
            InputText = "";
        }

        private bool OnCanExecuteSendMessageCommand(object o)
        {
            return !string.IsNullOrWhiteSpace(InputText);
        }

        #endregion

        #region SendImageCommand

        private RelayCommand _sendImageCommand;

        public RelayCommand SendImageCommand => _sendImageCommand ??= new RelayCommand(OnExecuteSendImageCommand);

        private void OnExecuteSendImageCommand(object o)
        {
            var imageSource = $"https://api.adorable.io/avatars/128/Agent{UserId}.png";
            Messages.Add(new TelegramMockMessage("", true, imageSource));
            _gameHostService.SendMessage(UserId, imagePath: imageSource);
        }

        #endregion

        #region MakePlayerCommand

        private bool _hasRunMakePlayerCommand;

        public bool HasRunMakePlayerCommand
        {
            get { return _hasRunMakePlayerCommand; }
            set
            {
                if (value != _hasRunMakePlayerCommand)
                {
                    _hasRunMakePlayerCommand = value;
                    OnPropertyChanged();
                }
            }
        }



        private string _playerName;

        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                if (value != _playerName)
                {
                    _playerName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _agentName;

        public string AgentName
        {
            get { return _agentName; }
            set
            {
                if (value != _agentName)
                {
                    _agentName = value;
                    OnPropertyChanged();
                }
            }
        }

        

        private RelayCommand _makePlayerCommand;

        public RelayCommand MakePlayerCommand => _makePlayerCommand ??= new RelayCommand(OnExecuteMakePlayerCommand, OnCanExecuteMakePlayerCommand);

        private async void OnExecuteMakePlayerCommand(object o)
        {
            HasRunMakePlayerCommand = true;

            async Task Send(string msg)
            {
                Messages.Add(new TelegramMockMessage(msg, true));
                _gameHostService.SendMessage(UserId, msg);
                await Task.Delay(1500);
            }

            await Send("/start");
            await Send("/nySøknad");
            var randomName = RandomData.GetRandomName();
            PlayerName = randomName;
            await Send(randomName);
            //Regex match = new Regex("agentnavnet Agent (\\w+)");
            var matchCollection = Regex.Matches(Messages.LastOrDefault()?.Message ?? "", "agentnavnet Agent (\\w+)");
            AgentName = matchCollection.Last().Groups.Values.Last().Value;
            Messages.Last().Message.IndexOf("agentnavnet Agent");
            await Send("/ok");
            Messages.Add(new TelegramMockMessage(null, true, $"https://api.adorable.io/avatars/128/Agent{UserId}.png"));
            _gameHostService.SendMessage(UserId, "", $"https://api.adorable.io/avatars/128/Agent{UserId}.png");
            await Task.Delay(1500);
            await Send("/ok");
        }

        private bool OnCanExecuteMakePlayerCommand(object o)
        {
            return !HasRunMakePlayerCommand;
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
        public string ImagePath { get; set; }
        public bool ShowImage => ImagePath != null;

        public TelegramMockMessage(string message, bool mine, string imagePath = null)
        {
            Message = message;
            Mine = mine;
            ImagePath = imagePath;
        }
    }
}