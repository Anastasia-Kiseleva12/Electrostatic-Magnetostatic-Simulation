using ElectroMagSimulator.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace ElectroMagSimulator.Core
{
    public class GridGenerator : IGridGenerator
    {
        private IMesh? _mesh;
        private List<IGridArea>? _areas;
        private double _xStart;
        private double _yStart;

        public void Generate(IReadOnlyList<IGridArea> areas, IGridAxis xAxis, IGridAxis yAxis)
        {
            _areas = areas.ToList();
            _xStart = xAxis.Start;
            _yStart = yAxis.Start;

            var xSteps = CalculateSteps(_xStart, xAxis.Points, xAxis.HMin, xAxis.DH, xAxis.SH);
            var ySteps = CalculateSteps(_yStart, yAxis.Points, yAxis.HMin, yAxis.DH, yAxis.SH);

            Generate(xSteps, ySteps);
        }

        public void Generate(IReadOnlyList<double> xSteps, IReadOnlyList<double> ySteps)
        {
            if (_areas == null)
                throw new InvalidOperationException("Список областей не задан. Сначала вызовите перегруженный Generate с областями.");

            var xCoords = new List<double> { _xStart };
            foreach (var s in xSteps)
                xCoords.Add(xCoords.Last() + s);

            var yCoords = new List<double> { _yStart };
            foreach (var s in ySteps)
                yCoords.Add(yCoords.Last() + s);

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

                    // Центр элемента
                    double centerX = 0.25 * (xCoords[ix] + xCoords[ix + 1] + xCoords[ix] + xCoords[ix + 1]);
                    double centerY = 0.25 * (yCoords[iy] + yCoords[iy + 1] + yCoords[iy] + yCoords[iy + 1]);

                    int areaId = GetAreaIdForPoint(centerX, centerY);

                    elements.Add(new Element
                    {
                        Id = elemId++,
                        NodeIds = new[] { n0, n1, n2, n3 },
                        AreaId = areaId
                    });
                }
            }


            var materials = new List<Material>();
            _mesh = new SimpleMesh(nodes, elements, _areas, materials);
        }

        private List<double> CalculateSteps(double start, IReadOnlyList<double> innerPoints,
                                  IReadOnlyList<double> hmin, IReadOnlyList<double> dh,
                                  IReadOnlyList<int> sh)
        {
            var fullPoints = new List<double> { start };
            fullPoints.AddRange(innerPoints);
            var steps = new List<double>();

            for (int i = 0; i < fullPoints.Count - 1; i++)
            {
                double L = fullPoints[i + 1] - fullPoints[i];
                double h = hmin[i];
                double d = dh[i];
                int sgn = sh[i];

                int N = EstimateStepsCount(L, h, d);
                double sum = 0;
                double lastStep = 0;

                for (int n = 0; n < N; n++)
                {
                    double factor = Math.Pow(d, sgn == 1 ? n : (N - 1 - n));
                    lastStep = h * factor;
                    steps.Add(lastStep);
                    sum += lastStep;
                }

                if (Math.Abs(sum - L) > 1e-8)
                {
                    steps[steps.Count - 1] += L - sum;
                }
            }

            return steps;
        }
        private int EstimateStepsCount(double length, double h, double d)
        {
            if (Math.Abs(d - 1.0) < 1e-8)
                return Math.Max(1, (int)Math.Round(length / h));

            return Math.Max(1, (int)Math.Ceiling(Math.Log(1 + length * (d - 1) / h, d)));
        }

        public IMesh GetMesh()
        {
            if (_mesh == null)
                throw new InvalidOperationException("Сетка ещё не сгенерирована");

            return _mesh;
        }
        private int GetAreaIdForPoint(double x, double y)
        {
            for (int i = 0; i < _areas.Count; i++)
            {
                var a = _areas[i];
                if (x >= a.X0 && x <= a.X1 && y >= a.Y0 && y <= a.Y1)
                    return a.AreaId;  // ВАЖНО: брать Id из области, а не индекс i!
            }
            throw new Exception($"Точка ({x},{y}) не попала ни в одну область");
        }

    }
}