using ElectroMagSimulator.Models;
using System;
using System.Linq;

namespace ElectroMagSimulator.Core
{
    public abstract class BaseScalarFieldPostProcessor : IPostProcessor
    {
        protected IMesh _mesh;
        protected double[] _solution;

        public void LoadMesh(IMesh mesh, double[] solution)
        {
            _mesh = mesh;
            _solution = solution;
        }

        public abstract ProbePoint EvaluateAt(double x, double y);

        protected Element FindElementContaining(double x, double y)
        {
            foreach (var element in _mesh.Elements)
            {
                var nodes = element.NodeIds.Select(_mesh.GetNode).ToArray();

                double minX = nodes.Min(n => n.X);
                double maxX = nodes.Max(n => n.X);
                double minY = nodes.Min(n => n.Y);
                double maxY = nodes.Max(n => n.Y);

                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    return element;
            }
            throw new Exception($"Точка ({x}, {y}) не принадлежит ни одному элементу.");
        }

        protected double InterpolateA(Node[] nodes, double[] values, double x, double y)
        {
            double x0 = nodes[0].X;
            double y0 = nodes[0].Y;
            double hx = nodes[2].X - nodes[0].X;
            double hy = nodes[2].Y - nodes[0].Y;

            double xi = (x - x0) / hx;
            double eta = (y - y0) / hy;

            double[] N = new double[4];
            N[0] = (1 - xi) * (1 - eta);
            N[1] = xi * (1 - eta);
            N[2] = xi * eta;
            N[3] = (1 - xi) * eta;

            double result = 0;
            for (int i = 0; i < 4; i++)
                result += values[i] * N[i];

            return result;
        }

        protected (double dx, double dy) ComputeGradient(Node[] nodes, double[] values)
        {
            double hx = nodes[2].X - nodes[0].X;
            double hy = nodes[2].Y - nodes[0].Y;

            double dA_dx = ((values[1] + values[2]) - (values[0] + values[3])) / (2 * hx);
            double dA_dy = ((values[2] + values[3]) - (values[0] + values[1])) / (2 * hy);

            return (dA_dx, dA_dy);
        }
    }
}
