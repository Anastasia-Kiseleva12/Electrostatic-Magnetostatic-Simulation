using ElectroMagSimulator.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ElectroMagSimulator.Core
{
    public class SimpleMesh : IMesh
    {
        public IEnumerable<Node> Nodes => _nodes;
        public IEnumerable<Element> Elements => _elements;
        public int NodeCount => _nodes.Count;
        public int ElementCount => _elements.Count;

        private readonly List<Node> _nodes;
        private readonly List<Element> _elements;
        private readonly List<IGridArea> _areas;
        private readonly List<Material> _materials;

        // Новое: маппинг elementId → materialId
        private readonly Dictionary<int, int> _elementMaterialMap = new();

        public List<IGridArea> Areas => _areas;

        public SimpleMesh(
            List<Node> nodes,
            List<Element> elements,
            List<IGridArea> areas,
            List<Material> materials)
        {
            _nodes = nodes;
            _elements = elements;
            _areas = areas;
            _materials = materials;

            // Изначально: никто не имеет материала
            foreach (var elem in _elements)
            {
                elem.AreaId = -1;
            }
        }

        public Node GetNode(int index) => _nodes[index];

        public Element GetElement(int index) => _elements[index];

        /// Присвоение материала всем элементам, попавшим в геом.область
        public void AssignMaterialToArea(int areaId, int materialId)
        {
            var area = _areas.FirstOrDefault(a => a.AreaId == areaId);
            if (area == null)
            {
                Debug.WriteLine($"⚠️ Область с AreaId={areaId} не найдена");
                return;
            }

            foreach (var elem in _elements)
            {
                var nodes = elem.NodeIds.Select(id => GetNode(id)).ToArray();

                // Центр элемента
                double centerX = nodes.Average(n => n.X);
                double centerY = nodes.Average(n => n.Y);

                if (centerX >= area.X0 && centerX <= area.X1 &&
                    centerY >= area.Y0 && centerY <= area.Y1)
                {
                    _elementMaterialMap[elem.Id] = materialId;
                    elem.AreaId = areaId;  // Обновляем AreaId для отображения
                }
            }
        }

        /// Получение материала элемента по маппингу
        public Material? GetMaterialForElement(Element element)
        {
            if (_elementMaterialMap.TryGetValue(element.Id, out int materialId))
            {
                var mat = _materials.FirstOrDefault(m => m.AreaId == materialId);
                if (mat == null)
                    Debug.WriteLine($"⚠️ Материал с AreaId={materialId} не найден");
                return mat;
            }
            return null;
        }

        public List<Material> GetMaterials() => _materials;

        public void SetMaterials(List<Material> materials)
        {
            _materials.Clear();
            _materials.AddRange(materials);
        }

        /// Проверка: все ли элементы имеют назначенный материал
        public bool AllElementsHaveMaterial()
        {
            return _elements.All(e => _elementMaterialMap.ContainsKey(e.Id));
        }
    }
}
