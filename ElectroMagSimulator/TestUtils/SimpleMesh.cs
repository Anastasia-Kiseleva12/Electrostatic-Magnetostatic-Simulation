using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.TestUtils
{
    public class SimpleMesh : IMesh
    {
        public IEnumerable<Node> Nodes => _nodes;
        public IEnumerable<Element> Elements => _elements;
        public int NodeCount => _nodes.Count;
        public int ElementCount => _elements.Count;

        private List<Node> _nodes;
        private List<Element> _elements;

        public SimpleMesh(List<Node> nodes, List<Element> elements)
        {
            _nodes = nodes;
            _elements = elements;
        }

        public Node GetNode(int index) => _nodes[index];
        public Element GetElement(int index) => _elements[index];
    }
}
