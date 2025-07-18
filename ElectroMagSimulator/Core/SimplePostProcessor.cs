using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class SimplePostProcessor : BaseScalarFieldPostProcessor
    {
        public override ProbePoint EvaluateAt(double x, double y)
        {
            var element = FindElementContaining(x, y);
            if (element == null)
            {
                Debug.WriteLine($"Warning: No element found at ({x}, {y})");
                return new ProbePoint { X = x, Y = y, Az = double.NaN, Bx = double.NaN, By = double.NaN };
            }

            var nodes = element.NodeIds.Select(id => _mesh.GetNode(id)).ToArray();
            var values = element.NodeIds.Select(id => _solution[id]).ToArray();

            if (nodes.Length != 4 || values.Any(v => double.IsNaN(v)))
            {
                Debug.WriteLine($"Warning: Invalid nodes or values at ({x}, {y})");
                return new ProbePoint { X = x, Y = y, Az = double.NaN, Bx = double.NaN, By = double.NaN };
            }

            double az = InterpolateA(nodes, values, x, y);
            var (dA_dx, dA_dy) = ComputeGradient(nodes, values);

            var probe = new ProbePoint
            {
                X = x,
                Y = y,
                Az = az,
                Bx = -dA_dy,
                By = dA_dx
            };
            probe.Recalculate();
            return probe;

        }

    }

}
