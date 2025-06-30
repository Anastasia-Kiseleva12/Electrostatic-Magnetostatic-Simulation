using DynamicData;
using ElectroMagSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ElectroMagSimulator.Models
{
    public interface IGridGenerator
    {
        void Generate(double width, double height, double hx, double hy); // равномерная
        void Generate(IReadOnlyList<double> xSteps, IReadOnlyList<double> ySteps); // неравномерная
        IMesh GetMesh();
        IEnumerable<Node> GetNodes();
        IEnumerable<Element> GetElements();
    }
    public interface IMesh
    {
        IEnumerable<Node> Nodes { get; }
        IEnumerable<Element> Elements { get; }

        int NodeCount { get; }
        int ElementCount { get; }

        Node GetNode(int index);
        Element GetElement(int index);
    }
    public interface IMatrixAssembler
    {
        void Assemble(IMesh mesh, Material material, IRightPart source);
        double[,] GetMatrix();
        double[] GetRhs();
    }

    public interface ISolver
    {
        double[] Solve(SparseMatrix matrix, double[] rhs);
    }

    public interface IPostProcessor
    {
        void Visualize(double[] solution);
    }
    public class Node
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Element
    {
        public int Id { get; set; }
        public int[] NodeIds { get; set; }
        public int AreaId { get; set; }
    }

    public class Material
    {
        public double Mu { get; set; }
        public int AreaId { get; set; }
    }

    public interface IRightPart
    {
        double GetValueAt(Node node);
    }

}
