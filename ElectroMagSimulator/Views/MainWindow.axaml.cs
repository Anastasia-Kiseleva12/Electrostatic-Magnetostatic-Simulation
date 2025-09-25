using Avalonia.Controls;
using Avalonia;
using ElectroMagSimulator.ViewModels;
using System.Reactive.Linq;
using ReactiveUI;
using System.Linq;
using ElectroMagSimulator.Core;

namespace ElectroMagSimulator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.Maximized;

        DrawingCanvas.AreaClicked += area =>
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedMaterial != null)
            {
                vm.AssignMaterialToArea(area, vm.SelectedMaterial);
                DrawingCanvas.InvalidateVisual();
            }
        };
        DrawingCanvas.PointClicked += point =>
        {
            if (DataContext is MainWindowViewModel vm && vm.IsProbeMode && vm.PostProcessor != null)
            {
                var probe = vm.PostProcessor.EvaluateAt(point.X, point.Y);
                vm.AddProbePoint(point.X, point.Y, probe);

            }
        };

        this.DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel newVm)
            {
                AttachHandlers(newVm);

                DrawingCanvas.SelectedMaterial = newVm.SelectedMaterial;
                DrawingCanvas.IsMaterialPaintMode = newVm.IsMaterialPaintMode;
                DrawingCanvas.SetShowAreaBorders(newVm.ShowBoundaries);
                DrawingCanvas.UpdateCursor();
                newVm.ProbePoints.CollectionChanged += (_, __) =>
                {
                    DrawingCanvas.SetProbePoints(newVm.ProbePoints.Select(p => new Avalonia.Point(p.X, p.Y)));
                };
                newVm.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainWindowViewModel.SelectedMaterial))
                        DrawingCanvas.SelectedMaterial = newVm.SelectedMaterial;

                    if (args.PropertyName == nameof(MainWindowViewModel.IsMaterialPaintMode))
                    {
                        DrawingCanvas.IsMaterialPaintMode = newVm.IsMaterialPaintMode;
                        DrawingCanvas.UpdateCursor();
                    }

                    if (args.PropertyName == nameof(MainWindowViewModel.ShowBoundaries))
                        DrawingCanvas.SetShowAreaBorders(newVm.ShowBoundaries);
                    if (args.PropertyName == nameof(MainWindowViewModel.IsProbeMode))
                    {
                        DrawingCanvas.IsProbeMode = newVm.IsProbeMode;
                        DrawingCanvas.UpdateCursor();
                    }
                };
            }
        };
    }
    private CreateGridWindow? _gridWindow;

    private void AttachHandlers(MainWindowViewModel vm)
    {
        vm.GridGenerated += OnGridGenerated;
        vm.SolutionGenerated += OnSolutionGenerated;
        vm.CreateGridRequested += ShowCreateGridWindow;

        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SelectedMaterial))
                DrawingCanvas.SelectedMaterial = vm.SelectedMaterial;

            if (args.PropertyName == nameof(MainWindowViewModel.ShowBoundaries))
                DrawingCanvas.SetShowAreaBorders(vm.ShowBoundaries);
        };
    }

    private async void ShowCreateGridWindow()
    {
        if (_gridWindow == null || !_gridWindow.IsVisible)
        {
            _gridWindow = new CreateGridWindow();

            if (_gridWindow.DataContext is CreateGridViewModel vm && DataContext is MainWindowViewModel mainVm)
            {
                vm.GridSettingsConfirmed += (areas, xAxis, yAxis, isXMod, isYMod) =>
                {
                    // Больше ничего не подменяем здесь
                    if (DataContext is MainWindowViewModel mainVm)
                        mainVm.ApplyGridSettings(areas, xAxis, yAxis, isXMod, isYMod);
                };


                vm.RequestClose += _ => _gridWindow.Close();

                if (mainVm.LastAreas != null && mainVm.LastXAxis != null && mainVm.LastYAxis != null)
                    vm.LoadGridData(mainVm.LastAreas, mainVm.LastXAxis, mainVm.LastYAxis);
            }

            await _gridWindow.ShowDialog(this);
        }
        else
        {
            _gridWindow.Activate();
        }
    }

    private void OnGridGenerated(IMesh? mesh)
    {
        DrawingCanvas.SetMesh(mesh);
    }

    private void OnSolutionGenerated(double[] solution)
    {
        DrawingCanvas.SetSolution(solution);
    }

    private async void OnMaterialSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (vm.SelectedMaterial == null)
            return;

        if (vm.SelectedMaterial.Name == "+ Добавить материал...")
        {
            var dialog = new AddMaterialWindow();
            dialog.SetMode(vm.SelectedMode);
            var result = await dialog.ShowDialog<bool?>(this);
            if (result == true && dialog.Material is MaterialViewModel newMaterial)
            {
                // Назначаем новый уникальный AreaId
                int nextId = vm.Materials
                    .Where(m => m.AreaId >= 0)
                    .Select(m => m.AreaId)
                    .DefaultIfEmpty(-1)
                    .Max() + 1;

                newMaterial.AreaId = nextId;

                vm.Materials.Insert(vm.Materials.Count - 1, newMaterial);
                vm.SelectedMaterial = newMaterial;
            }
            else
            {
                vm.SelectedMaterial = vm.Materials.FirstOrDefault();
            }
        }
    }

}
