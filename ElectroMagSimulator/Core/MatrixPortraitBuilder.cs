using System;
using System.Collections.Generic;
using System.Linq;
using ElectroMagSimulator.Models;

public class MatrixPortraitBuilder
{
    public class MatrixPortrait
    {
        public int[] Ig { get; }
        public int[] Jg { get; }

        public MatrixPortrait(int[] ig, int[] jg)
        {
            Ig = ig;
            Jg = jg;
        }
    }

    public MatrixPortrait Build(IMesh mesh)
    {
        int nodeCount = mesh.NodeCount;

        // Список смежности для каждого узла
        var adjacency = new HashSet<int>[nodeCount];
        for (int i = 0; i < nodeCount; i++)
            adjacency[i] = new HashSet<int>();

        foreach (var element in mesh.Elements)
        {
            var nodes = element.NodeIds;

            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = 0; j < nodes.Length; j++)
                {
                    int row = nodes[i];
                    int col = nodes[j];

                    if (col < row)
                        adjacency[row].Add(col);
                }
            }
        }

        var ig = new int[nodeCount + 1];
        var jgList = new List<int>();

        for (int i = 0; i < nodeCount; i++)
        {
            var neighbors = adjacency[i].ToList();
            neighbors.Sort(); 

            jgList.AddRange(neighbors);
            ig[i + 1] = jgList.Count;
        }

        return new MatrixPortrait(ig, jgList.ToArray());
    }
}
