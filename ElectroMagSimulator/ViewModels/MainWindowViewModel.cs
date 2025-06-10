using ReactiveUI;
using System.Windows.Input;

namespace ElectroMagSimulator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string[] SimulationModes { get; } = new[] { "Электростатика", "Магнитостатика" };

        private string _selectedMode = "Электростатика";
        public string SelectedMode
        {
            get => _selectedMode;
            set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
        }

        private double _areaWidth = 10;
        public double AreaWidth
        {
            get => _areaWidth;
            set => this.RaiseAndSetIfChanged(ref _areaWidth, value);
        }

        private double _areaHeight = 10;
        public double AreaHeight
        {
            get => _areaHeight;
            set => this.RaiseAndSetIfChanged(ref _areaHeight, value);
        }

        private double _sourceValue = 1.0;
        public double SourceValue
        {
            get => _sourceValue;
            set => this.RaiseAndSetIfChanged(ref _sourceValue, value);
        }

        private double _materialProperty = 1.0;
        public double MaterialProperty
        {
            get => _materialProperty;
            set => this.RaiseAndSetIfChanged(ref _materialProperty, value);
        }

        public ICommand BuildGridCommand { get; }
        public ICommand SolveCommand { get; }
        public ICommand ClearCommand { get; }

        public MainWindowViewModel()
        {
            BuildGridCommand = ReactiveCommand.Create(BuildGrid);
            SolveCommand = ReactiveCommand.Create(Solve);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        private void BuildGrid() { /* TODO */ }

        private void Solve() { /* TODO */ }

        private void Clear() { /* TODO */ }
    }
}
