using ElectroMagSimulator.Core;
using ElectroMagSimulator.Models;
using ElectroMagSimulator.TestUtils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace ElectroMagSimulator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string[] SimulationModes { get; } = { "Электростатика", "Магнитостатика" };
        public string[] GridTypes { get; } = { "Равномерная", "Неравномерная" };

        private string _selectedMode = "Электростатика";
        public string SelectedMode
        {
            get => _selectedMode;
            set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
        }

        private string _areaWidth = "10";
        public string AreaWidth
        {
            get => _areaWidth;
            set => this.RaiseAndSetIfChanged(ref _areaWidth, value);
        }

        private string _areaHeight = "10";
        public string AreaHeight
        {
            get => _areaHeight;
            set => this.RaiseAndSetIfChanged(ref _areaHeight, value);
        }

        private string _sourceValue = "1.0";
        public string SourceValue
        {
            get => _sourceValue;
            set => this.RaiseAndSetIfChanged(ref _sourceValue, value);
        }

        private string _materialProperty = "1.0";
        public string MaterialProperty
        {
            get => _materialProperty;
            set => this.RaiseAndSetIfChanged(ref _materialProperty, value);
        }

        public string StepX { get; set; } = "1.0";
        public string StepY { get; set; } = "1.0";

        public string CustomStepsX { get; set; } = "1,1,2";
        public string CustomStepsY { get; set; } = "1,0.5,1.5";

        private string _selectedGridType = "Равномерная";
        public string SelectedGridType
        {
            get => _selectedGridType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedGridType, value);
                this.RaisePropertyChanged(nameof(IsUniformGrid));
            }
        }

        public bool IsUniformGrid => SelectedGridType == "Равномерная";

        public event Action<IMesh>? GridGenerated;
        public event Action<double[]>? SolutionGenerated;
        private IMesh? _mesh;

        public ICommand BuildGridCommand { get; }
        public ICommand SolveCommand { get; }
        public ICommand ClearCommand { get; }

        public MainWindowViewModel()
        {
            BuildGridCommand = ReactiveCommand.Create(BuildGrid);
            SolveCommand = ReactiveCommand.Create(Solve);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        private void BuildGrid()
        {
            try
            {
                if (!TryParse(AreaWidth, out double areaWidth) ||
                    !TryParse(AreaHeight, out double areaHeight) ||
                    !TryParse(SourceValue, out double sourceValue) ||
                    !TryParse(MaterialProperty, out double materialProperty))
                {
                    Console.WriteLine("Ошибка: Некорректные числовые значения области или свойств.");
                    return;
                }

                var generator = new GridGenerator();

                if (IsUniformGrid)
                {
                    if (!TryParse(StepX, out double hx) || !TryParse(StepY, out double hy))
                    {
                        Console.WriteLine("Ошибка: Некорректные значения шагов.");
                        return;
                    }

                    generator.Generate(areaWidth, areaHeight, hx, hy);
                }
                else
                {
                    var xSteps = ParseDoubleList(CustomStepsX);
                    var ySteps = ParseDoubleList(CustomStepsY);

                    if (xSteps == null || ySteps == null)
                    {
                        Console.WriteLine("Ошибка: Некорректные значения неравномерных шагов.");
                        return;
                    }

                    generator.Generate(xSteps, ySteps);
                }

                _mesh = generator.GetMesh();
                GridGenerated?.Invoke(_mesh);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при построении сетки: {ex.Message}");
            }
        }
        private IProblem? _problem;

        private void Solve()
        {
            if (_mesh == null)
                return;

            if (!TryParse(MaterialProperty, out double materialProperty) ||
                !TryParse(SourceValue, out double sourceValue))
            {
                Console.WriteLine("Ошибка: Некорректные числовые данные для задачи.");
                return;
            }

            var source = new ConstantRightPart(sourceValue);

            if (SelectedMode == "Электростатика")
            {
                _problem = new ElectrostaticProblem(materialProperty, source);
            }
            else if (SelectedMode == "Магнитостатика")
            {
                _problem = new MagnetostaticProblem(materialProperty, source);
            }
            else
            {
                Console.WriteLine("Ошибка: Неизвестный режим задачи.");
                return;
            }

            _problem.Assemble(_mesh);
            var solution = _problem.Solve();

            SolutionGenerated?.Invoke(solution);
        }



        private void Clear()
        {
            _mesh = null;
            GridGenerated?.Invoke(null);
        }

        private bool TryParse(string str, out double result)
        {
            return double.TryParse(str.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private List<double>? ParseDoubleList(string str)
        {
            try
            {
                return str.Split(',')
                    .Select(s => double.Parse(s.Trim().Replace(',', '.'), CultureInfo.InvariantCulture))
                    .ToList();
            }
            catch
            {
                return null;
            }
        }
    }
}
