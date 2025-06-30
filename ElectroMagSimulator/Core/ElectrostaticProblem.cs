using ElectroMagSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroMagSimulator.Core
{
    public class ElectrostaticProblem : IProblem
    {
        private MatrixAssembler _assembler;
        private SparseMatrix _matrix;
        private double[] _rhs;
        private double _epsilon;
        private IRightPart _source;
        private IMesh _mesh;
        private MatrixPortraitBuilder.MatrixPortrait _portrait;

        public ElectrostaticProblem(double epsilon, IRightPart source)
        {
            _epsilon = epsilon;
            _source = source;
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleElectrostatics(mesh, _epsilon, _source, _portrait);
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            return new CGSolver().Solve(_matrix, _rhs);
        }

        public void PostProcess(double[] solution)
        {
            // Например, передать на визуализацию
        }
    }
}
