using System.Collections.Generic;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.Core
{
    public class MagnetostaticProblem : IProblem
    {
        private MatrixAssembler _assembler;
        private SparseMatrix _matrix;
        private double[] _rhs;
        private IMesh _mesh;
        private MatrixPortraitBuilder.MatrixPortrait _portrait;

        private double[] _solutionA; 

        public double[] MagneticPotential => _solutionA; 

        public MagnetostaticProblem()
        {
            
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleMagnetostatics(mesh, _portrait);
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            _solutionA = new CGSolver().Solve(_matrix, _rhs);
            return _solutionA;
        }

        public void PostProcess(double[] solution)
        {
            // Здесь в будущем можно визуализировать изолинии A или поле индукции B
        }
    }
}
