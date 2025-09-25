using Avalonia.Controls;
using Avalonia.Interactivity;
using ElectroMagSimulator.ViewModels;
using System;

namespace ElectroMagSimulator.Views
{
    public partial class CreateGridWindow : Window
    {
        public CreateGridWindow()
        {
            InitializeComponent();

            var vm = new CreateGridViewModel();
            DataContext = vm;
            vm.RequestClose += result => Close(result);
        }

    }
}
