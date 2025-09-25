using ElectroMagSimulator.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var xCoords = CalculateCoordinates(_xStart, xAxis.Points, xAxis.HMin, xAxis.DH, xAxis.SH, xAxis.DoubleMode);
            var yCoords = CalculateCoordinates(_yStart, yAxis.Points, yAxis.HMin, yAxis.DH, yAxis.SH, yAxis.DoubleMode);

            GenerateFromCoords(xCoords, yCoords);
        }

        private List<double> CalculateCoordinates(double start, IReadOnlyList<double> points,
            IReadOnlyList<double> hmin, IReadOnlyList<double> dh,
            IReadOnlyList<int> sh, double doubleMode)
        {
            var coords = new SortedSet<double>();
            double end = start;

            for (int i = 0; i < points.Count; i++)
            {
                double beg = end;
                end = points[i];
                double sign = sh[i];
                double rb = sign > 0 ? beg : end;
                double re = sign > 0 ? end : beg;

                coords.Add(rb);

                double k = dh[i];
                double h = hmin[i] * sign;

                if (doubleMode > 0.0)
                    h /= doubleMode * 2.0;

                double newPos = rb + h;
                while (sign * newPos < re * sign)
                {
                    coords.Add(newPos);
                    h *= k;
                    newPos += h;
                }

                coords.Add(re);
            }

            return coords.ToList();
        }

        private void GenerateFromCoords(IReadOnlyList<double> xCoords, IReadOnlyList<double> yCoords)
        {
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
            _mesh = new SimpleMesh(nodes, elements, _areas!, materials);
        }

        public IMesh GetMesh()
        {
            if (_mesh == null)
                throw new InvalidOperationException("Сетка ещё не сгенерирована");

            return _mesh;
        }

        private int GetAreaIdForPoint(double x, double y)
        {
            for (int i = 0; i < _areas!.Count; i++)
            {
                var a = _areas[i];
                if (x >= a.X0 && x <= a.X1 && y >= a.Y0 && y <= a.Y1)
                    return a.AreaId;
            }
            throw new Exception($"Точка ({x},{y}) не попала ни в одну область");
        }
    }
}
