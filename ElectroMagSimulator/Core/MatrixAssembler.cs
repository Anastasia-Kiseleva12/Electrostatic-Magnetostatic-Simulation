using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ElectroMagSimulator.Core
{
    public class MatrixAssembler
    {
        private SparseMatrix _matrix;
        private double[] _rhs;

        public void AssembleElectrostatics(
            IMesh mesh,
            IReadOnlyList<Material> materials,
            MatrixPortraitBuilder.MatrixPortrait portrait,
            IRightPart source)
        {
            foreach (var mat in materials)
                Debug.WriteLine($"Material: AreaId={mat.AreaId}, Mu={mat.Mu}, TokJ={mat.TokJ}");

            AssembleInternal(mesh, materials, portrait, isMagnetostatic: false, source);
        }

        public void AssembleMagnetostatics(
            IMesh mesh,
            IReadOnlyList<Material> materials,
            MatrixPortraitBuilder.MatrixPortrait portrait)
        {
            foreach (var mat in materials)
                Debug.WriteLine($"Material: AreaId={mat.AreaId}, Mu={mat.Mu}, TokJ={mat.TokJ}");

            AssembleInternal(mesh, materials, portrait, isMagnetostatic: true, source: null);
        }

        public SparseMatrix GetMatrix() => _matrix;

        public double[] GetRhs() => _rhs;

        private void AssembleInternal(
            IMesh mesh,
            IReadOnlyList<Material> materials,
            MatrixPortraitBuilder.MatrixPortrait portrait,
            bool isMagnetostatic,
            IRightPart source)
        {
            int nodeCount = mesh.NodeCount;
            _matrix = new SparseMatrix(portrait);
            _rhs = new double[nodeCount];

            foreach (var element in mesh.Elements)
            {
                var nodes = element.NodeIds.Select(id => mesh.GetNode(id)).ToArray();

                var material = materials.FirstOrDefault(m => m.AreaId == element.AreaId);
                Debug.WriteLine($"Element {element.Id} has AreaId {element.AreaId}");
                Debug.WriteLine("Materials list: " + string.Join(", ", materials.Select(m => $"Id={m.AreaId}")));
                if (material == null)
                    throw new Exception($"Не найден материал для области {element.AreaId}");

                double lambda = 1.0 / material.Mu;
                double sourceValue = isMagnetostatic ? material.TokJ : 1.0;

                var localMatrix = new double[4, 4];
                var localRhs = new double[4];

                ComputeLocalMatrixAndRhs(nodes, lambda, sourceValue, localMatrix, localRhs, isMagnetostatic, source);

                for (int i = 0; i < 4; i++)
                {
                    int globalI = nodes[i].Id;
                    _rhs[globalI] += localRhs[i];

                    for (int j = 0; j < 4; j++)
                    {
                        int globalJ = nodes[j].Id;
                        if (globalI == globalJ || globalI > globalJ)
                        {
                            _matrix.AddTo(globalI, globalJ, localMatrix[i, j]);
                        }
                    }
                }
            }
        }

        private void ComputeLocalMatrixAndRhs(
            Node[] nodes,
            double lambda,
            double sourceValue,
            double[,] localMatrix,
            double[] localRhs,
            bool isMagnetostatic,
            IRightPart source)
        {
            double x1 = nodes[0].X;
            double y1 = nodes[0].Y;
            double x2 = nodes[1].X;
            double y2 = nodes[2].Y;

            double hx = x2 - x1;
            double hy = y2 - y1;

            double[,] Gx = {
                {  2, -2,  1, -1 },
                { -2,  2, -1,  1 },
                {  1, -1,  2, -2 },
                { -1,  1, -2,  2 }
            };

            double[,] Gy = {
                {  2,  1, -2, -1 },
                {  1,  2, -1, -2 },
                { -2, -1,  2,  1 },
                { -1, -2,  1,  2 }
            };

            double[,] C = {
                {  4,  2,  2,  1 },
                {  2,  4,  1,  2 },
                {  2,  1,  4,  2 },
                {  1,  2,  2,  4 }
            };

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    localMatrix[i, j] = lambda * ((hy / hx) * Gx[i, j] / 6.0 + (hx / hy) * Gy[i, j] / 6.0);

            double factorRhs = hx * hy / 36.0;

            if (isMagnetostatic)
            {
                for (int i = 0; i < 4; i++)
                {
                    localRhs[i] = 0;
                    for (int j = 0; j < 4; j++)
                        localRhs[i] += C[i, j];
                    localRhs[i] *= factorRhs * sourceValue;
                }
            }
            else // электростатика
            {
                double[] f = new double[4];
                for (int i = 0; i < 4; i++)
                    f[i] = source.GetValueAt(nodes[i]);

                for (int i = 0; i < 4; i++)
                {
                    localRhs[i] = 0;
                    for (int j = 0; j < 4; j++)
                        localRhs[i] += C[i, j] * f[j];
                    localRhs[i] *= factorRhs;
                }
            }
        }
    }
}
