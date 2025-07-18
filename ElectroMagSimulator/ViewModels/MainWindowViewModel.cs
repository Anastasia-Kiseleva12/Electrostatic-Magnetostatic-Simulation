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
        private bool _isProbePanelOpen;
        public bool IsProbePanelOpen
        {
            get => _isProbePanelOpen;
            set => this.RaiseAndSetIfChanged(ref _isProbePanelOpen, value);
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
        private bool _isProbeMode;
        public bool IsProbeMode
        {
            get => _isProbeMode;
            set => this.RaiseAndSetIfChanged(ref _isProbeMode, value);
        }
        public ObservableCollection<ProbePoint> ProbePoints { get; } = new();
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
        public ReactiveCommand<Unit, Unit> ToggleProbeModeCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleProbePanelCommand { get; }

        public event Action? CreateGridRequested;
        public IPostProcessor? PostProcessor { get; private set; }

        public MainWindowViewModel()
        {
            Materials.Add(new MaterialViewModel("Магнит", 1.0, "Red", 0, 0.0));
            Materials.Add(new MaterialViewModel("Воздух", 2.0, "Green", 1, 0.0));
            Materials.Add(new MaterialViewModel("+ Добавить материал...", 0.0, "Gray", -1, 0.0));

            SelectedMaterial = Materials.FirstOrDefault();
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
            ToggleProbeModeCommand = ReactiveCommand.Create(() =>
            {
                IsProbeMode = !IsProbeMode;
            });
            ToggleProbePanelCommand = ReactiveCommand.Create(() =>
            {
                IsProbePanelOpen = !IsProbePanelOpen;
            });
            Materials.CollectionChanged += (_, _) => SyncMaterialsToMesh();
        }
        public void AddProbePoint(double x, double y, ProbePoint point)
        {
            ProbePoints.Add(point);
            Debug.WriteLine($"Добавлена точка: X={point.X}, Y={point.Y}, Az={point.Az}, Bx={point.Bx}, By={point.By}, BAbs={point.BAbs}");

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
                simpleMesh.AssignMaterialToArea(area.AreaId, materialVm.AreaId);
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

            var materials = Materials
            .Where(m => m.AreaId >= 0)
            .Select(m => m.ToMaterial()) // если есть метод ToMaterial()
            .ToList();


            var source = new ConstantRightPart(sourceValue);

            if (SelectedMode == "Электростатика")
            {
                _problem = new ElectrostaticProblem(materials, source);
            }
            else if (SelectedMode == "Магнитостатика")
            {
                _problem = new MagnetostaticProblem(materials); // ← теперь принимает materials
            }
            else
            {
                Console.WriteLine("Ошибка: Неизвестный режим задачи.");
                return;
            }

            _problem.Assemble(_mesh);
            var solution = _problem.Solve();
            SolutionGenerated?.Invoke(solution);

            if (_mesh != null && solution != null)
            {
                PostProcessor = new SimplePostProcessor();
                PostProcessor.LoadMesh(_mesh, solution);
            }

        }

        private void Clear()
        {
            _mesh = null;
            GridGenerated?.Invoke(null);
            PostProcessor = null;
        }
        private bool TryParse(string str, out double result)
        {
            return double.TryParse(str.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

    }
}
