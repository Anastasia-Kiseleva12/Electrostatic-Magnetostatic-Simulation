using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class GridArea : IGridArea
    {
        public double X0 { get; set; }
        public double X1 { get; set; }
        public double Y0 { get; set; }
        public double Y1 { get; set; }
        public int AreaId { get; set; }
    }

}
