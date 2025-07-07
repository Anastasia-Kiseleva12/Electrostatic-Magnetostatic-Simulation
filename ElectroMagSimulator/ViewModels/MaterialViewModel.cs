using ElectroMagSimulator.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ElectroMagSimulator.ViewModels
{
    public class MaterialViewModel : ViewModelBase
    {
        private string _name;
        private string _propertyValueStr;
        private string _color;
        private int _areaId;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        /// <summary>
        /// Строка для безопасного биндинга
        /// </summary>
        private string _tokJStr;

        public string TokJStr
        {
            get => _tokJStr;
            set => this.RaiseAndSetIfChanged(ref _tokJStr, value);
        }
        public double TokJ
        {
            get
            {
                return double.TryParse(TokJStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double res)
                    ? res : double.NaN;
            }
        }

        public string PropertyValueStr
        {
            get => _propertyValueStr;
            set => this.RaiseAndSetIfChanged(ref _propertyValueStr, value);
        }

        public string Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }

        public int AreaId
        {
            get => _areaId;
            set => this.RaiseAndSetIfChanged(ref _areaId, value);
        }

        public ObservableCollection<string> AvailableColors { get; } = new()
        {
            "Red",
            "Green",
            "Blue",
            "Yellow",
            "Black"
        };

        /// <summary>
        /// Получаем числовое значение или NaN если некорректно
        /// </summary>
        public double PropertyValue
        {
            get
            {
                return double.TryParse(PropertyValueStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double res)
                    ? res : double.NaN;
            }
        }

        public MaterialViewModel(string name, double propertyValue, string color, int areaId, double tokJ = 0.0)
        {
            _name = name;
            _propertyValueStr = propertyValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _tokJStr = tokJ.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _color = color;
            _areaId = areaId;
        }


        public MaterialViewModel()
        {
            _propertyValueStr = "";
            _tokJStr = "";
            _color = AvailableColors[0];
        }


        public Material ToMaterial()
        {
            return new Material
            {
                AreaId = AreaId,
                Mu = PropertyValue,
                TokJ = TokJ,
                Color = Color
            };
        }
        public override string ToString()
        {
            return $"{Name} (ID: {AreaId}, Свойство: {PropertyValueStr}, Цвет: {Color})";
        }
    }
}
