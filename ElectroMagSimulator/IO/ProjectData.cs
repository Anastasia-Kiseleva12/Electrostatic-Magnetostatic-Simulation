using ElectroMagSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.IO
{
    public class ProjectData
    {
        public List<Node> Nodes { get; set; }
        public List<Element> Elements { get; set; }
        public List<Material> Materials { get; set; }
        public List<GridArea> Areas { get; set; }
        public List<ProbePoint> ProbePoints { get; set; }
        public Dictionary<int, int> AreaMaterialMap { get; set; }
        public GridAxis? XAxis { get; set; }  
        public GridAxis? YAxis { get; set; }  
    }
}
