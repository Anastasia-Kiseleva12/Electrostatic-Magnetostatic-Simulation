using ElectroMagSimulator.Models;
using ElectroMagSimulator.Core;

class Program
{
    public class RightPart2X : IRightPart
    {
        public double GetValueAt(Node node)
        {
            return 2 * node.X;
        }
    }

    static void Main(string[] args)
    {
        // Создание узлов
        var nodes = new List<Node>
        {
            new Node { Id = 0, X = 1, Y = 1 },
            new Node { Id = 1, X = 2, Y = 1 },
            new Node { Id = 2, X = 1, Y = 2 },
            new Node { Id = 3, X = 2, Y = 2 }
        };

        // Создание элемента
        var elements = new List<Element>
        {
            new Element { Id = 0, NodeIds = new[] { 0, 1, 2, 3 } }
        };

       
    }
}
