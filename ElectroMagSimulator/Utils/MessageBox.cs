using Avalonia.Controls;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Utils
{
    public static class MessageBox
    {
        public static async Task Show(Window owner, string message, string title = "Сообщение")
        {
            var dialog = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var okButton = new Button
            {
                Content = "ОК",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                Width = 60
            };

            okButton.Click += (_, _) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(10),
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(0, 0, 0, 10)
                    },
                    okButton
                }
            };

            await dialog.ShowDialog(owner);
        }
    }
}
