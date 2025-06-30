using System;
using System.Linq;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.Core
{
    public class MatrixAssembler
    {
        private SparseMatrix _matrix;
        private double[] _rhs;

        public void AssembleElectrostatics(IMesh mesh, double epsilon, IRightPart source, MatrixPortraitBuilder.MatrixPortrait portrait)
        {
            AssembleInternal(mesh, 1.0 / epsilon, source, portrait, addMassMatrix: true);
        }

        public void AssembleMagnetostatics(IMesh mesh, double mu, IRightPart source, MatrixPortraitBuilder.MatrixPortrait portrait)
        {
            AssembleInternal(mesh, 1.0 / mu, source, portrait, addMassMatrix: false);
        }

        public SparseMatrix GetMatrix() => _matrix;

        public double[] GetRhs() => _rhs;

        private void AssembleInternal(IMesh mesh, double lambda, IRightPart source, MatrixPortraitBuilder.MatrixPortrait portrait, bool addMassMatrix)
        {
            int nodeCount = mesh.NodeCount;
            _matrix = new SparseMatrix(portrait);
            _rhs = new double[nodeCount];

            foreach (var element in mesh.Elements)
            {
                var localMatrix = new double[4, 4];
                var localRhs = new double[4];
                var nodes = element.NodeIds.Select(id => mesh.GetNode(id)).ToArray();

                ComputeLocalMatrixAndRhs(nodes, lambda, source, localMatrix, localRhs, addMassMatrix);

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

        private void ComputeLocalMatrixAndRhs(Node[] nodes, double lambda, IRightPart source, double[,] localMatrix, double[] localRhs, bool addMassMatrix)
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
                { 4, 2, 2, 1 },
                { 2, 4, 1, 2 },
                { 2, 1, 4, 2 },
                { 1, 2, 2, 4 }
            };

            double[,] localG = new double[4, 4];
            double[,] localM = new double[4, 4];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    localG[i, j] = lambda * ((hy / hx) * Gx[i, j] / 6.0 + (hx / hy) * Gy[i, j] / 6.0);

            if (addMassMatrix)
            {
                double gamma = 2.0;
                double factor = gamma * hx * hy / 36.0;
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        localM[i, j] = factor * C[i, j];
            }

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    localMatrix[i, j] = localG[i, j] + (addMassMatrix ? localM[i, j] : 0);

            double[] f = new double[4];
            for (int i = 0; i < 4; i++)
                f[i] = source.GetValueAt(nodes[i]);

            double factorRhs = hx * hy / 36.0;
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
