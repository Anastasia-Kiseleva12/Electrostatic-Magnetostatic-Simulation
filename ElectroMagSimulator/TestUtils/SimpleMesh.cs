using ElectroMagSimulator.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class SimpleMesh : IMesh
{
    private readonly List<Node> _nodes;
    private readonly List<Element> _elements;
    private readonly List<IGridArea> _areas;
    private List<Material> _materials;

    // areaId → materialId (назначение материала областям)
    private readonly Dictionary<int, int> _areaMaterialMap = new();

    public IEnumerable<Node> Nodes => _nodes;
    public IEnumerable<Element> Elements => _elements;
    public int NodeCount => _nodes.Count;
    public int ElementCount => _elements.Count;
    public List<IGridArea> Areas => _areas;

    public SimpleMesh(List<Node> nodes, List<Element> elements, List<IGridArea> areas, List<Material> materials)
    {
        _nodes = nodes;
        _elements = elements;
        _areas = areas;
        _materials = materials;
    }

    public Node GetNode(int index) => _nodes[index];
    public Element GetElement(int index) => _elements[index];

    public List<Material> GetMaterials() => _materials;

    public void SetMaterials(List<Material> materials) => _materials = materials;

    public Dictionary<int, int> GetAreaMaterialMap() => _areaMaterialMap;

    public void SetAreaMaterialMap(Dictionary<int, int> map)
    {
        _areaMaterialMap.Clear();
        foreach (var kvp in map)
            _areaMaterialMap[kvp.Key] = kvp.Value;
    }

    public void AssignMaterialToArea(int areaId, int materialId)
    {
        _areaMaterialMap[areaId] = materialId;
    }
    /// Получаем материал элемента через area → materialId
    public Material? GetMaterialForElement(Element element)
    {
        if (_areaMaterialMap.TryGetValue(element.AreaId, out int materialId))
        {
            return _materials.FirstOrDefault(m => m.MaterialId == materialId);
        }
        return null;
    }
    /// Проверяем, есть ли назначение для всех областей
    public bool AllAreasHaveMaterial()
    {
        return _areas.All(a => _areaMaterialMap.ContainsKey(a.AreaId));
    }
}
