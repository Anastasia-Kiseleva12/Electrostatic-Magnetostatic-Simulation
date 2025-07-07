using ElectroMagSimulator.Core;
using ElectroMagSimulator.Models;
using ElectroMagSimulator.TestUtils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace ElectroMagSimulator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MaterialViewModel> Materials { get; } = new();
        private MaterialViewModel? _selectedMaterial;
        public MaterialViewModel? SelectedMaterial
        {
            get => _selectedMaterial;
            set => this.RaiseAndSetIfChanged(ref _selectedMaterial, value);
        }
        public string[] SimulationModes { get; } = { "Электростатика", "Магнитостатика" };

        private string _selectedMode = "Магнитостатика";
        public string SelectedMode
        {
            get => _selectedMode;
            set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
        }

        private string _gridButtonText = "📐 Задать сетку";
        public string GridButtonText
        {
            get => _gridButtonText;
            set => this.RaiseAndSetIfChanged(ref _gridButtonText, value);
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
        private bool _showBoundaries;
        public bool ShowBoundaries
        {
            get => _showBoundaries;
            set => this.RaiseAndSetIfChanged(ref _showBoundaries, value);
        }
        public ReactiveCommand<Unit, Unit> ToggleBoundariesCommand { get; }

        private bool _isMaterialPaintMode;
        public bool IsMaterialPaintMode
        {
            get => _isMaterialPaintMode;
            set => this.RaiseAndSetIfChanged(ref _isMaterialPaintMode, value);
        }

        public ReactiveCommand<Unit, Unit> ActivateMaterialPaintCommand { get; }

        public List<IGridArea>? LastAreas { get; private set; }
        public IGridAxis? LastXAxis { get; private set; }
        public IGridAxis? LastYAxis { get; private set; }

        public event Action<IMesh>? GridGenerated;
        public event Action<double[]>? SolutionGenerated;
        private IMesh? _mesh;
        private IProblem? _problem;
        public ICommand BuildGridCommand { get; }
        public ICommand SolveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CreateGridCommand { get; }
        public event Action? CreateGridRequested;
        public MainWindowViewModel()
        {
            Materials.Add(new MaterialViewModel("Магнит", 1.0, "Red", 0, 0.0));
            Materials.Add(new MaterialViewModel("Воздух", 2.0, "Green", 1, 0.0));
            Materials.Add(new MaterialViewModel("+ Добавить материал...", 0.0, "Gray", -1, 0.0));

            SelectedMaterial = Materials.FirstOrDefault();

            BuildGridCommand = ReactiveCommand.Create(BuildGrid);
            SolveCommand = ReactiveCommand.Create(Solve);
            ClearCommand = ReactiveCommand.Create(Clear);
            CreateGridCommand = ReactiveCommand.Create(OnCreateGridClick);
            ToggleBoundariesCommand = ReactiveCommand.Create(() =>
            {
                ShowBoundaries = !ShowBoundaries;
            });
            ActivateMaterialPaintCommand = ReactiveCommand.Create(() =>
            {
                IsMaterialPaintMode = !IsMaterialPaintMode;
            });

            Materials.CollectionChanged += (_, _) => SyncMaterialsToMesh();
        }
        private void SyncMaterialsToMesh()
        {
            if (_mesh is SimpleMesh simpleMesh)
            {
                var converted = Materials
                    .Where(m => m.AreaId >= 0)
                    .Select(m => m.ToMaterial())
                    .ToList();

                Debug.WriteLine("Список материалов для сетки:");
                foreach (var mat in converted)
                    Debug.WriteLine($"AreaId: {mat.AreaId}, Color: {mat.Color}");

                simpleMesh.SetMaterials(converted);
            }
        }

        public void AssignMaterialToArea(IGridArea area, MaterialViewModel materialVm)
        {
            if (_mesh is SimpleMesh simpleMesh)
            {
                foreach (var element in simpleMesh.Elements)
                {
                    var nodes = element.NodeIds.Select(id => simpleMesh.GetNode(id)).ToArray();

                    double minX = nodes.Min(n => n.X);
                    double maxX = nodes.Max(n => n.X);
                    double minY = nodes.Min(n => n.Y);
                    double maxY = nodes.Max(n => n.Y);

                    bool inside = minX >= area.X0 && maxX <= area.X1 &&
                                  minY >= area.Y0 && maxY <= area.Y1;

                    if (inside)
                        element.AreaId = materialVm.AreaId;
                }

                SyncMaterialsToMesh();   // Обновляем список материалов внутри сетки
                GridGenerated?.Invoke(_mesh);  // Чтобы визуализация обновилась
            }
        }

        private void OnCreateGridClick()
        {
            CreateGridRequested?.Invoke();
        }
        public void ApplyGridSettings(List<IGridArea> areas, IGridAxis xAxis, IGridAxis yAxis)
        {
            LastAreas = areas;
            LastXAxis = xAxis;
            LastYAxis = yAxis;

            var generator = new GridGenerator();
            generator.Generate(areas, xAxis, yAxis);
            _mesh = generator.GetMesh();

            SyncMaterialsToMesh();

            GridGenerated?.Invoke(_mesh);
            GridButtonText = "✏️ Редактировать сетку";
        }

        private void BuildGrid()
        {
            try
            {
                if (!TryParse(SourceValue, out double sourceValue) ||
                    !TryParse(MaterialProperty, out double materialProperty))
                {
                    Console.WriteLine("Ошибка: Некорректные числовые значения области или свойств.");
                    return;
                }

                var generator = new GridGenerator();
                _mesh = generator.GetMesh();

                if (_mesh is SimpleMesh simpleMesh)
                {
                    simpleMesh.SetMaterials(Materials
                        .Where(m => m.AreaId >= 0)
                        .Select(m => m.ToMaterial())
                        .ToList());
                }
                SyncMaterialsToMesh();
                GridGenerated?.Invoke(_mesh);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при построении сетки: {ex.Message}");
            }
        }
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

            var materials = new List<Material>
            {
                new Material { AreaId = 0, Mu = materialProperty }
            };

            var source = new ConstantRightPart(sourceValue);

            if (SelectedMode == "Электростатика")
            {
                _problem = new ElectrostaticProblem(materials, source);
            }
            else if (SelectedMode == "Магнитостатика")
            {
                _problem = new MagnetostaticProblem(materials, source);
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

    }
}
