using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.Core
{
    public class CGSolver : ISolver
    {
        private readonly double _tolerance; 
        private readonly int _maxIterations;

        public CGSolver(double tolerance = 1e-8, int maxIterations = 10000)
        {
            _tolerance = tolerance;
            _maxIterations = maxIterations;
        }

        public double[] Solve(SparseMatrix matrix, double[] rhs)
        {
            int n = matrix.Size;
            double[] x = new double[n];
            double[] r = new double[n];
            double[] p = new double[n];
            double[] Ap = new double[n];

            matrix.Multiply(x, r);

            for (int i = 0; i < n; i++)
            {
                r[i] = rhs[i] - r[i];
                p[i] = r[i];
            }

            double rsOld = Dot(r, r);
            double rsNew = rsOld;
            double bNorm = Math.Sqrt(Dot(rhs, rhs));

            if (bNorm < 1e-20)
                bNorm = 1.0;

            for (int k = 0; k < _maxIterations; k++)
            {
                matrix.Multiply(p, Ap);

                double alpha = rsOld / Dot(p, Ap);

                for (int i = 0; i < n; i++)
                {
                    x[i] += alpha * p[i];
                    r[i] -= alpha * Ap[i];
                }

                rsNew = Dot(r, r);
                double error = Math.Sqrt(rsNew) / bNorm;

                if (error < _tolerance)
                {
                    Console.WriteLine($"Converged in {k + 1} iterations with relative error = {error:E}");
                    break;
                }

                double beta = rsNew / rsOld;

                for (int i = 0; i < n; i++)
                    p[i] = r[i] + beta * p[i];

                rsOld = rsNew;
            }

            return x;
        }

        private double Dot(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }
    }
}
