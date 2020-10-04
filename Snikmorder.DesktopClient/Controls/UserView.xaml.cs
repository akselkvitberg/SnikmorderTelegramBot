using Snikmorder.DesktopClient.GameMock;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Snikmorder.DesktopClient.Controls
{
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TelegramMockUser user)
            {
                user.Messages.CollectionChanged += (o, args) =>
                {
                    items.ScrollIntoView();
                };
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is TelegramMockUser user && e.Source is Hyperlink hyperlink)
            {
                user.InputText = hyperlink.Tag as string;
                user.SendMessageCommand.Execute(null);
            }
        }
    }

    public static class Extensions
    {
        public static void ScrollIntoView(this ItemsControl control, object item)
        {
            var framework = control.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            framework?.BringIntoView();
        }

        public static void ScrollIntoView(this ItemsControl control)
        {
            int count = control.Items.Count;
            if (count == 0) { return; }
            object item = control.Items[count - 1];
            control.ScrollIntoView(item);
        }
    }
}