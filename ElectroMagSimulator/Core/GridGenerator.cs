using ElectroMagSimulator.Models;
using ElectroMagSimulator.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectroMagSimulator.Core
{
    public  class GridGenerator : IGridGenerator
    {
        private IMesh? _mesh;

        public void Generate(double width, double height, double hx, double hy)
        {
            if (hx <= 0 || hy <= 0 || width <= 0 || height <= 0)
                throw new ArgumentException("Параметры сетки некорректны");

            var xSteps = Enumerable.Repeat(hx, (int)Math.Ceiling(width / hx)).ToList();
            var ySteps = Enumerable.Repeat(hy, (int)Math.Ceiling(height / hy)).ToList();

            Generate(xSteps, ySteps);
        }
        public void Generate(IReadOnlyList<double> xSteps, IReadOnlyList<double> ySteps)
        {
            if (xSteps.Any(s => s <= 0) || ySteps.Any(s => s <= 0))
                throw new ArgumentException("Шаги сетки должны быть положительными");

            var xCoords = xSteps.Aggregate(new List<double> { 0.0 }, (acc, s) =>
            {
                acc.Add(acc.Last() + s);
                return acc;
            });

            var yCoords = ySteps.Aggregate(new List<double> { 0.0 }, (acc, s) =>
            {
                acc.Add(acc.Last() + s);
                return acc;
            });

            var nodes = new List<Node>();
            int nodeId = 0;
            for (int iy = 0; iy < yCoords.Count; iy++)
            {
                for (int ix = 0; ix < xCoords.Count; ix++)
                {
                    nodes.Add(new Node
                    {
                        Id = nodeId++,
                        X = xCoords[ix],
                        Y = yCoords[iy]
                    });
                }
            }

            var elements = new List<Element>();
            int cols = xCoords.Count;
            int elemId = 0;

            for (int iy = 0; iy < yCoords.Count - 1; iy++)
            {
                for (int ix = 0; ix < xCoords.Count - 1; ix++)
                {
                    int n0 = iy * cols + ix;
                    int n1 = n0 + 1;
                    int n2 = n0 + cols;
                    int n3 = n2 + 1;

                    elements.Add(new Element
                    {
                        Id = elemId++,
                        NodeIds = new[] { n0, n1, n2, n3 }
                    });
                }
            }

            _mesh = new SimpleMesh(nodes, elements);
        }

        public IMesh GetMesh()
        {
            if (_mesh == null)
                throw new InvalidOperationException("Сетка ещё не сгенерирована");

            return _mesh;
        }

        public IEnumerable<Node> GetNodes() => _mesh?.Nodes ?? Enumerable.Empty<Node>();

        public IEnumerable<Element> GetElements() => _mesh?.Elements ?? Enumerable.Empty<Element>();
    }
}
