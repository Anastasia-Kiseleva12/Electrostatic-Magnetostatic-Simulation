using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.TestUtils
{
    public class SimpleRightPart : IRightPart
    {
        private double _value;

        public SimpleRightPart(double value)
        {
            _value = value;
        }

        public double GetValueAt(Node node) => _value;
    }
}
