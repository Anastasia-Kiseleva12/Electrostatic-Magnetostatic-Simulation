using Avalonia.Controls;
using Avalonia.Interactivity;
using ElectroMagSimulator.Utils;
using ElectroMagSimulator.ViewModels;
using System;
using System.Diagnostics;

namespace ElectroMagSimulator.Views
{
    public partial class AddMaterialWindow : Window
    {
        public MaterialViewModel? Material { get; private set; }

        public AddMaterialWindow()
        {
            InitializeComponent();
            DataContext = new MaterialViewModel();
        }

        public void SetMode(string selectedMode)
        {
            Debug.WriteLine($"SetMode вызван с параметром: {selectedMode}");

            if (selectedMode == "Электростатика")
            {
                PermeabilityLabel.Text = "Диэлектрическая проницаемость (ε):";
                TokJPanel.IsVisible = false;
            }
            else if (selectedMode == "Магнитостатика")
            {
                PermeabilityLabel.Text = "Магнитная проницаемость (μ):";
                TokJPanel.IsVisible = true;
            }
        }
        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MaterialViewModel vm)
            {
                if (string.IsNullOrWhiteSpace(vm.Name))
                {
                    ShowError("Введите название материала.");
                    return;
                }

                if (double.IsNaN(vm.PropertyValue) || vm.PropertyValue <= 0)
                {
                    ShowError("Проницаемость должна быть положительным числом.");
                    return;
                }

                if (TokJPanel.IsVisible && double.IsNaN(vm.TokJ))
                {
                    ShowError("Плотность тока должна быть числом (можно с минусом или с экспонентой).");
                    return;
                }

                if (string.IsNullOrWhiteSpace(vm.Color))
                {
                    ShowError("Выберите цвет.");
                    return;
                }

                Material = vm;
                Close(true);
            }
        }
        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private async void ShowError(string message)
        {
            await MessageBox.Show(this, message, "Ошибка");
        }
    }
}
