using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class MagnetostaticPostProcessor : BaseScalarFieldPostProcessor
    {
        public override ProbePoint EvaluateAt(double x, double y)
        {
            var element = FindElementContaining(x, y);
            var nodes = element.NodeIds.Select(_mesh.GetNode).ToArray();
            var values = element.NodeIds.Select(id => _solution[id]).ToArray();

            double A = InterpolateA(nodes, values, x, y);
            var (dx, dy) = ComputeGradient(nodes, values);

            return new ProbePoint
            {
                X = x,
                Y = y,
                Az = A,
                Bx = dy,  
                By = -dx  
            };
        }
    }

}
