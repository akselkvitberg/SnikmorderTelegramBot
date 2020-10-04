using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Snikmorder.DesktopClient.Controls
{
    public static class LinkHelper
    {
        private static readonly Regex CommandRegex = new Regex("/\\w+");

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(LinkHelper),
            new PropertyMetadata(null, OnTextChanged)
        );

        public static string GetText(DependencyObject d)
        {
            return d.GetValue(TextProperty) as string;
        }

        public static void SetText(DependencyObject d, string value)
        {
            d.SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock textBlock))
                return;

            textBlock.Inlines.Clear();

            var newText = (string)e.NewValue;
            if (string.IsNullOrEmpty(newText))
                return;

            // Find all commands using a regular expression
            var lastPos = 0;
            foreach (Match match in CommandRegex.Matches(newText))
            {
                // Copy raw string from the last position up to the match
                if (match.Index != lastPos)
                {
                    var rawText = newText.Substring(lastPos, match.Index - lastPos);
                    textBlock.Inlines.Add(new Run(rawText));
                }

                // Create a hyperlink for the match
                var link = new Hyperlink(new Run(match.Value))
                {
                    Tag = (match.Value),
                    Focusable = false,
                };
                //link.Click += OnUrlClick;

                textBlock.Inlines.Add(link);

                // Update the last matched position
                lastPos = match.Index + match.Length;
            }

            // Finally, copy the remainder of the string
            if (lastPos < newText.Length)
                textBlock.Inlines.Add(new Run(newText.Substring(lastPos)));
        }

        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            //var link = (Hyperlink)sender;
            //// Do something with link.NavigateUri like:
            //Process.Start(link.NavigateUri.ToString());
        }
    }
}