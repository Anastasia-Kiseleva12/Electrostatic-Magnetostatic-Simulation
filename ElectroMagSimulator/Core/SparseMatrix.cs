using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class SparseMatrix
    {
        public double[] Di { get; }
        public double[] Gg { get; }
        public int[] Ig { get; }
        public int[] Jg { get; }

        public int Size => Di.Length;

        public SparseMatrix(MatrixPortraitBuilder.MatrixPortrait portrait)
        {
            Ig = portrait.Ig;
            Jg = portrait.Jg;

            Di = new double[Ig.Length - 1];
            Gg = new double[Jg.Length];
        }

        public void AddTo(int row, int col, double value)
        {
            if (row == col)
            {
                Di[row] += value;
            }
            else
            {
                if (col > row)
                {
                    int temp = row;
                    row = col;
                    col = temp;
                }

                for (int k = Ig[row]; k < Ig[row + 1]; k++)
                {
                    if (Jg[k] == col)
                    {
                        Gg[k] += value;
                        return;
                    }
                }

                throw new Exception($"Ошибка сборки: элемент ({row},{col}) вне портрета матрицы.");
            }
        }

        public void Multiply(double[] x, double[] result)
        {
            int n = Di.Length;

            // Обнуляем результат
            for (int i = 0; i < n; i++)
                result[i] = 0.0;

            // Диагональная часть
            for (int i = 0; i < n; i++)
                result[i] += Di[i] * x[i];

            // Внедиагональная часть (только нижний треугольник, по портрету)
            for (int i = 0; i < n; i++)
            {
                for (int k = Ig[i]; k < Ig[i + 1]; k++)
                {
                    int j = Jg[k];
                    double value = Gg[k];

                    result[i] += value * x[j];
                    result[j] += value * x[i];
                }
            }
        }

    }

}
