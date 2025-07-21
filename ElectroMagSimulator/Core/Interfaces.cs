using ReactiveUI;
using System;
using System.Collections.Generic;

namespace ElectroMagSimulator.Core
{
    // Описание области
    public interface IGridArea
    {
        int AreaId { get; }
        double X0 { get; }
        double X1 { get; }
        double Y0 { get; }
        double Y1 { get; }
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
        int NodeCount { get; }
        int ElementCount { get; }

        Node GetNode(int index);
        Element GetElement(int index);

        List<IGridArea> Areas { get; }

        List<Material> GetMaterials();
        void SetMaterials(List<Material> materials);

        Dictionary<int, int> GetAreaMaterialMap();
        void SetAreaMaterialMap(Dictionary<int, int> map);
        Material? GetMaterialForElement(Element element);

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
        void LoadMesh(IMesh mesh, double[] solution);
        ProbePoint EvaluateAt(double x, double y);
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
        public int MaterialId { get; set; }  // УНИКАЛЬНЫЙ id материала
        public string Name { get; set; }
        public double Mu { get; set; }
        public double TokJ { get; set; }
        public string Color { get; set; }
    }

    public interface IRightPart
    {
        double GetValueAt(Node node);
    }
    public class ProbePoint : ReactiveObject
    {
        private double _x;
        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        private double _y;
        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        private double _az;
        public double Az
        {
            get => _az;
            set => this.RaiseAndSetIfChanged(ref _az, value);
        }

        private double _bx;
        public double Bx
        {
            get => _bx;
            set => this.RaiseAndSetIfChanged(ref _bx, value);
        }

        private double _by;
        public double By
        {
            get => _by;
            set => this.RaiseAndSetIfChanged(ref _by, value);
        }

        private double _bAbs;
        public double BAbs
        {
            get => _bAbs;
            set => this.RaiseAndSetIfChanged(ref _bAbs, value);
        }

        public void Recalculate()
        {
            BAbs = Math.Sqrt(Bx * Bx + By * By);
        }
    }

}

