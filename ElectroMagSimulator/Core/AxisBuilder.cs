using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public static class AxisBuilder
    {
        public static IGridAxis FromCoords(IEnumerable<double> coords)
        {
            var sorted = coords.Distinct().OrderBy(v => v).ToList();
            if (sorted.Count < 2)
                throw new System.InvalidOperationException("Недостаточно узлов для построения оси.");

            var start = sorted[0];
            var points = sorted.Skip(1).ToList();

            var hmin = new List<double>();
            var dh = new List<double>();
            var sh = new List<int>();
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                double step = sorted[i + 1] - sorted[i];
                hmin.Add(step);
                dh.Add(1.0);
                sh.Add(1);
            }

            return new GridAxis
            {
                Start = start,
                Points = points,
                HMin = hmin,
                DH = dh,
                SH = sh,
                DoubleMode = 0
            };
        }
    }
}
