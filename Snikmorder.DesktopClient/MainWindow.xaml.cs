using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snikmorder.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TelegramMockClient _telegramMockClient;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            var httpClient = new HttpClient();
            _telegramMockClient = new TelegramMockClient(httpClient);

            for (int i = 0; i < 5; i++)
            {
                Users.Add(new TelegramMockUser(i, $"Name {i}", _telegramMockClient));
            }
        }


        public ObservableCollection<TelegramMockUser> Users { get; } = new ObservableCollection<TelegramMockUser>();


    }

}