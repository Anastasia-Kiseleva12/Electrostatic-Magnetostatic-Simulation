using System;
using System.Collections.Generic;
using System.Linq;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.Core
{
    public class MagnetostaticProblem : IProblem
    {
        private MatrixAssembler _assembler;
        private SparseMatrix _matrix;
        private double[] _rhs;
        private double _mu;
        private IRightPart _source;
        private IMesh _mesh;
        private MatrixPortraitBuilder.MatrixPortrait _portrait;

        public MagnetostaticProblem(double mu, IRightPart source)
        {
            _mu = mu;
            _source = source;
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleMagnetostatics(mesh, _mu, _source, _portrait);
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            return new CGSolver().Solve(_matrix, _rhs);
        }

        public void PostProcess(double[] solution)
        {
            // Например, изолинии или визуализация B
        }
    }

}
