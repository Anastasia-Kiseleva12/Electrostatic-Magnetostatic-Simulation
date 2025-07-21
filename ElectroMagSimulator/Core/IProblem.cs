using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public interface IProblem
    {
        void Assemble(IMesh mesh);   // Сборка матрицы и правой части
        double[] Solve();            // Решение задачи
        void PostProcess(double[] solution); // Визуализация, анализ
    }
}
