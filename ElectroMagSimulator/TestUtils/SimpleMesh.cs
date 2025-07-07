using ElectroMagSimulator.Models;
using System;
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

        public IReadOnlyList<Material> Materials => _materials;
        public IReadOnlyList<IGridArea> Areas => _areas;

        private List<Node> _nodes;
        private List<Element> _elements;
        private List<Material> _materials;
        private List<IGridArea> _areas;

        public SimpleMesh(List<Node> nodes, List<Element> elements, List<Material> materials, List<IGridArea> areas)
        {
            _nodes = nodes;
            _elements = elements;
            _materials = materials;
            _areas = areas;
        }

        public Node GetNode(int index) => _nodes[index];

        public Element GetElement(int index) => _elements[index];

        public Material GetMaterialForElement(Element element)
        {
            if (element.AreaId == -1)
                return null;

            var material = _materials.FirstOrDefault(m => m.AreaId == element.AreaId);

            if (material == null)
                Debug.WriteLine($"⚠️ Материал не найден для элемента с AreaId = {element.AreaId}");

            return material;
        }


        public List<Material> GetMaterials() => _materials;

        public void SetMaterials(List<Material> materials)
        {
            _materials = materials;
        }
    }
}
