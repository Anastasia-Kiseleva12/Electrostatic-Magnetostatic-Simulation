using ElectroMagSimulator.Core;
using System.Collections.Generic;

namespace ElectroMagSimulator.Models
{
    // Описание области
    public interface IGridArea
    {
        double X0 { get; }
        double X1 { get; }
        double Y0 { get; }
        double Y1 { get; }
        Material Material { get; }
    }

    // Описание разбиения по одной оси
    public interface IGridAxis
    {
        double Start { get; }
        IReadOnlyList<double> Points { get; }      // Границы
        IReadOnlyList<double> HMin { get; }        // Минимальные шаги
        IReadOnlyList<double> DH { get; }          // Коэффициенты разрядки
        IReadOnlyList<int> SH { get; }             // Знаки разрядки
        int DoubleMode { get; }                    // 0 - нет, 1 - удвоение, 2 - учетверение
    }

    public interface IGridAxisGenerator
    {
        IReadOnlyList<double> GenerateAxis(IEnumerable<IGridArea> areas, bool isXAxis);
    }
    // Генератор сетки
    public interface IGridGenerator
    {
        void Generate(IReadOnlyList<IGridArea> areas, IGridAxis xAxis, IGridAxis yAxis);
        IMesh GetMesh();
    }

    public interface IMesh
    {
        IEnumerable<Node> Nodes { get; }
        IEnumerable<Element> Elements { get; }
        IReadOnlyList<IGridArea> Areas { get; }

        int NodeCount { get; }
        int ElementCount { get; }

        Node GetNode(int index);
        Element GetElement(int index);
        Material GetMaterialForElement(Element element);
    }

    // Сборка глобальной матрицы
    public interface IMatrixAssembler
    {
        void Assemble(IMesh mesh, IRightPart source);
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

    // Базовые классы
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
        public int AreaId { get; set; }  // Индекс области, к которой принадлежит элемент
    }

    public class Material
    {
        public double Mu { get; set; }
        public double TokJ { get; set; }
        public int AreaId { get; set; }
        public string Color { get; set; }
    }

    public interface IRightPart
    {
        double GetValueAt(Node node);
    }
}
