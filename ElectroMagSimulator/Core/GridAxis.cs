using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class GridAxis : IGridAxis
    {
        public double Start { get; set; }
        public IReadOnlyList<double> Points { get; set; } = new List<double>();
        public IReadOnlyList<double> HMin { get; set; } = new List<double>();
        public IReadOnlyList<double> DH { get; set; } = new List<double>();
        public IReadOnlyList<int> SH { get; set; } = new List<int>();
        public int DoubleMode { get; set; }
    }

}
