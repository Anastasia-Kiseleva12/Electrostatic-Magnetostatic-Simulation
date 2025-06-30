using System;
using System.Collections.Generic;
using ElectroMagSimulator.Models;
using ElectroMagSimulator.Core;
using ElectroMagSimulator.TestUtils;
using static ElectroMagSimulator.Models.Interfaces;

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

        // Создание сетки
        var mesh = new SimpleMesh(nodes, elements);

        // Портрет матрицы
        var portraitBuilder = new MatrixPortraitBuilder();
        var portrait = portraitBuilder.Build(mesh);

        // Материал и источник
        var material = new Material { Mu = 1.0 };
        var source = new RightPart2X();

        // Сборка матрицы
        var assembler = new MatrixAssembler();
        assembler.Assemble(mesh, material, source, portrait);

        var matrix = assembler.GetMatrix();
        var rhs = assembler.GetRhs();

        // Вывод разреженной матрицы
        Console.WriteLine("Диагональные элементы:");
        for (int i = 0; i < matrix.Di.Length; i++)
            Console.WriteLine($"di[{i}] = {matrix.Di[i]:F3}");

        Console.WriteLine("\nВнедиагональные элементы (по портрету):");
        for (int i = 0; i < matrix.Gg.Length; i++)
            Console.WriteLine($"gg[{i}] = {matrix.Gg[i]:F3}");

        Console.WriteLine("\nВектор правой части:");
        for (int i = 0; i < rhs.Length; i++)
            Console.WriteLine($"rhs[{i}] = {rhs[i]:F3}");

        Console.ReadLine();
    }
}
