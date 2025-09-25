using Avalonia.Data.Converters;
using ElectroMagSimulator.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;

namespace ElectroMagSimulator.ViewModels
{
    public class CreateGridViewModel : ViewModelBase
    {
        public event Action<bool>? RequestClose;

        private int _areaCount = 1;
        public int AreaCount
        {
            get => _areaCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _areaCount, value);
                UpdateAreas();
            }
        }

        public ObservableCollection<AreaViewModel> Areas { get; } = new();

        private bool _doubleToX;
        public bool DoubleToX
        {
            get => _doubleToX;
            set
            {
                this.RaiseAndSetIfChanged(ref _doubleToX, value);
                if (!_isLoading) IsXAxisModified = true;   // важно
            }
        }

        private bool _doubleToY;
        public bool DoubleToY
        {
            get => _doubleToY;
            set
            {
                this.RaiseAndSetIfChanged(ref _doubleToY, value);
                if (!_isLoading) IsYAxisModified = true;   // важно
            }
        }


        private bool _isXConfigVisible;
        public bool IsXConfigVisible
        {
            get => _isXConfigVisible;
            set => this.RaiseAndSetIfChanged(ref _isXConfigVisible, value);
        }

        private bool _isYConfigVisible;
        public bool IsYConfigVisible
        {
            get => _isYConfigVisible;
            set => this.RaiseAndSetIfChanged(ref _isYConfigVisible, value);
        }

        public ReactiveCommand<Unit, Unit> ConfigureXCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfigureYCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public GridAxisSettings XSettings { get; } = new();
        public GridAxisSettings YSettings { get; } = new();

        private bool _isXAxisModified = true;  // по умолчанию true, чтобы не забыли включить
        public bool IsXAxisModified
        {
            get => _isXAxisModified;
            set => this.RaiseAndSetIfChanged(ref _isXAxisModified, value);
        }

        private bool _isYAxisModified = true;
        public bool IsYAxisModified
        {
            get => _isYAxisModified;
            set => this.RaiseAndSetIfChanged(ref _isYAxisModified, value);
        }

        public CreateGridViewModel()
        {
            ConfigureXCommand = ReactiveCommand.Create(ConfigureX);
            ConfigureYCommand = ReactiveCommand.Create(ConfigureY);
            ConfirmCommand = ReactiveCommand.Create(OnConfirm);
            CancelCommand = ReactiveCommand.Create(OnCancel);
            UpdateAreas();
            HookAxisDirtyTracking();
        }
        private void HookAxisDirtyTracking()
        {
            void trackX() { if (!_isLoading) IsXAxisModified = true; }
            void trackY() { if (!_isLoading) IsYAxisModified = true; }

            XSettings.PropertyChanged += (_, __) => trackX();
            XSettings.XPoints.CollectionChanged += (_, __) => trackX();
            XSettings.HMin.CollectionChanged += (_, __) => trackX();
            XSettings.DH.CollectionChanged += (_, __) => trackX();
            XSettings.SH.CollectionChanged += (_, __) => trackX();

            YSettings.PropertyChanged += (_, __) => trackY();
            YSettings.XPoints.CollectionChanged += (_, __) => trackY();
            YSettings.HMin.CollectionChanged += (_, __) => trackY();
            YSettings.DH.CollectionChanged += (_, __) => trackY();
            YSettings.SH.CollectionChanged += (_, __) => trackY();
        }
        private void UpdateAreas()
        {
            while (Areas.Count < AreaCount)
                Areas.Add(new AreaViewModel());

            while (Areas.Count > AreaCount)
                Areas.RemoveAt(Areas.Count - 1);
        }
        private bool _isLoading;
        public void LoadGridData(List<IGridArea> areas, IGridAxis xAxis, IGridAxis yAxis)
        {
            _isLoading = true;
            AreaCount = areas.Count;
            Areas.Clear();

            try
            {
                foreach (var area in areas)
                {
                    Areas.Add(new AreaViewModel
                    {
                        X0 = area.X0.ToString(CultureInfo.InvariantCulture),
                        X1 = area.X1.ToString(CultureInfo.InvariantCulture),
                        Y0 = area.Y0.ToString(CultureInfo.InvariantCulture),
                        Y1 = area.Y1.ToString(CultureInfo.InvariantCulture),
                    });
                }

                DoubleToX = xAxis.DoubleMode > 0;
                DoubleToY = yAxis.DoubleMode > 0;

                // X
                XSettings.X0 = xAxis.Start.ToString(CultureInfo.InvariantCulture);
                XSettings.KolX = (xAxis.Points.Count + 1).ToString();  // учитываем левую границу + внутренние точки

                XSettings.XPoints.Clear();
                foreach (var p in xAxis.Points)
                    XSettings.XPoints.Add(p.ToString(CultureInfo.InvariantCulture));

                XSettings.HMin.Clear();
                foreach (var h in xAxis.HMin)
                    XSettings.HMin.Add(h.ToString(CultureInfo.InvariantCulture));

                XSettings.DH.Clear();
                foreach (var dh in xAxis.DH)
                    XSettings.DH.Add(dh.ToString(CultureInfo.InvariantCulture));

                XSettings.SH.Clear();
                foreach (var sh in xAxis.SH)
                    XSettings.SH.Add(sh.ToString());

                // Y
                YSettings.X0 = yAxis.Start.ToString(CultureInfo.InvariantCulture);
                YSettings.KolX = (yAxis.Points.Count + 1).ToString();

                YSettings.XPoints.Clear();
                foreach (var p in yAxis.Points)
                    YSettings.XPoints.Add(p.ToString(CultureInfo.InvariantCulture));

                YSettings.HMin.Clear();
                foreach (var h in yAxis.HMin)
                    YSettings.HMin.Add(h.ToString(CultureInfo.InvariantCulture));

                YSettings.DH.Clear();
                foreach (var dh in yAxis.DH)
                    YSettings.DH.Add(dh.ToString(CultureInfo.InvariantCulture));

                YSettings.SH.Clear();
                foreach (var sh in yAxis.SH)
                    YSettings.SH.Add(sh.ToString());

                IsXConfigVisible = true;
                IsYConfigVisible = true;
            }
            finally
            {
                IsXAxisModified = false;
                IsYAxisModified = false;
                _isLoading = false;
            }
        }
        private void ConfigureX()
        {
            if (Areas.Count == 0)
                return;

            var lefts = Areas.Select(a => double.TryParse(a.X0.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0).ToList();
            var rights = Areas.Select(a => double.TryParse(a.X1.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0).ToList();

            var allPoints = lefts.Concat(rights).Distinct().OrderBy(x => x).ToList();

            if (allPoints.Count == 0)
                return;

            var minX = allPoints.First();
            var remainingPoints = allPoints.Skip(1).ToList();

            XSettings.X0 = minX.ToString(CultureInfo.InvariantCulture);
            XSettings.KolX = (remainingPoints.Count + 1).ToString();

            XSettings.XPoints.Clear();
            foreach (var p in remainingPoints)
                XSettings.XPoints.Add(p.ToString(CultureInfo.InvariantCulture));

            XSettings.HMin.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                XSettings.HMin.Add("1.0");

            XSettings.DH.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                XSettings.DH.Add("1.0");

            XSettings.SH.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                XSettings.SH.Add("1");

            IsXConfigVisible = true;
            IsXAxisModified = true;
        }
        private void ConfigureY()
        {
            if (Areas.Count == 0)
                return;

            var bottoms = Areas.Select(a => double.TryParse(a.Y0.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0).ToList();
            var tops = Areas.Select(a => double.TryParse(a.Y1.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0).ToList();

            var allPoints = bottoms.Concat(tops).Distinct().OrderBy(y => y).ToList();

            if (allPoints.Count == 0)
                return;

            var minY = allPoints.First();
            var remainingPoints = allPoints.Skip(1).ToList();

            YSettings.X0 = minY.ToString(CultureInfo.InvariantCulture);
            YSettings.KolX = (remainingPoints.Count + 1).ToString();

            YSettings.XPoints.Clear();
            foreach (var p in remainingPoints)
                YSettings.XPoints.Add(p.ToString(CultureInfo.InvariantCulture));

            YSettings.HMin.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                YSettings.HMin.Add("1.0");

            YSettings.DH.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                YSettings.DH.Add("1.0");

            YSettings.SH.Clear();
            for (int i = 0; i < remainingPoints.Count; i++)
                YSettings.SH.Add("1");

            IsYConfigVisible = true;
            IsYAxisModified = true;
        }


        public event Action<List<IGridArea>, IGridAxis, IGridAxis, bool, bool>? GridSettingsConfirmed;

        private void OnConfirm()
        {
            GridSettingsConfirmed?.Invoke(
    Areas.Select((a, index) => a.ToGridArea(index)).ToList(),
    XSettings.ToGridAxis(DoubleToX ? 1 : 0),
    YSettings.ToGridAxis(DoubleToY ? 1 : 0),
    IsXAxisModified,
    IsYAxisModified
);


            RequestClose?.Invoke(true);
        }
        private void OnCancel()
        {
            RequestClose?.Invoke(false);
        }
    }
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AreaViewModel : ViewModelBase
    {
        private string _x0 = "0";
        private string _x1 = "0";
        private string _y0 = "0";
        private string _y1 = "0";

        public int Index { get; set; } 

        public string X0
        {
            get => _x0;
            set => this.RaiseAndSetIfChanged(ref _x0, value);
        }

        public string X1
        {
            get => _x1;
            set => this.RaiseAndSetIfChanged(ref _x1, value);
        }

        public string Y0
        {
            get => _y0;
            set => this.RaiseAndSetIfChanged(ref _y0, value);
        }

        public string Y1
        {
            get => _y1;
            set => this.RaiseAndSetIfChanged(ref _y1, value);
        }

        public IGridArea ToGridArea(int areaId)
        {
            return new GridArea
            {
                AreaId = areaId,
                X0 = double.Parse(X0.Replace(',', '.'), CultureInfo.InvariantCulture),
                X1 = double.Parse(X1.Replace(',', '.'), CultureInfo.InvariantCulture),
                Y0 = double.Parse(Y0.Replace(',', '.'), CultureInfo.InvariantCulture),
                Y1 = double.Parse(Y1.Replace(',', '.'), CultureInfo.InvariantCulture)
            };
        }

    }

    public class GridAxisSettings : ViewModelBase
    {
        private string _x0 = "0";
        public string X0
        {
            get => _x0;
            set => this.RaiseAndSetIfChanged(ref _x0, value);
        }

        private string _kolX = "0";
        public string KolX
        {
            get => _kolX;
            set => this.RaiseAndSetIfChanged(ref _kolX, value);
        }

        public ObservableCollection<string> XPoints { get; set; } = new();
        public ObservableCollection<string> HMin { get; set; } = new();
        public ObservableCollection<string> DH { get; set; } = new();
        public ObservableCollection<string> SH { get; set; } = new();

        public GridAxisSettings()
        {
            XPoints.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(PointsString));
            HMin.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(HMinString));
            DH.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(DHString));
            SH.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(SHString));
        }

        public string PointsString
        {
            get => string.Join(", ", XPoints);
            set
            {
                XPoints.Clear();
                foreach (var s in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    XPoints.Add(s.Trim());
            }
        }

        public string HMinString
        {
            get => string.Join(", ", HMin);
            set
            {
                HMin.Clear();
                foreach (var s in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    HMin.Add(s.Trim());
            }
        }

        public string DHString
        {
            get => string.Join(", ", DH);
            set
            {
                DH.Clear();
                foreach (var s in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    DH.Add(s.Trim());
            }
        }

        public string SHString
        {
            get => string.Join(", ", SH);
            set
            {
                SH.Clear();
                foreach (var s in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    SH.Add(s.Trim());
            }
        }
        public IGridAxis ToGridAxis(int doubleMode)
        {
            return new GridAxis
            {
                Start = double.Parse(X0.Replace(',', '.'), CultureInfo.InvariantCulture),
                Points = XPoints.Select(p => double.Parse(p.Replace(',', '.'), CultureInfo.InvariantCulture)).ToList(),
                HMin = HMin.Select(p => double.Parse(p.Replace(',', '.'), CultureInfo.InvariantCulture)).ToList(),
                DH = DH.Select(p => double.Parse(p.Replace(',', '.'), CultureInfo.InvariantCulture)).ToList(),
                SH = SH.Select(p => int.Parse(p)).ToList(),
                DoubleMode = doubleMode
            };
        }
    }
}
