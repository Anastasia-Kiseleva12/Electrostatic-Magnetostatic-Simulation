using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class ConstantRightPart : IRightPart
    {
        private readonly double _value;

        public ConstantRightPart(double value)
        {
            _value = value;
        }

        public double GetValueAt(Node node)
        {
            return _value;
        }
    }

}
