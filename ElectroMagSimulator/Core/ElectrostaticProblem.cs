using System.Collections.Generic;
using ElectroMagSimulator.Models;

namespace ElectroMagSimulator.Core
{
    public class ElectrostaticProblem : IProblem
    {
        private MatrixAssembler _assembler;
        private SparseMatrix _matrix;
        private double[] _rhs;
        private List<Material> _materials;
        private IRightPart _source;
        private IMesh _mesh;
        private MatrixPortraitBuilder.MatrixPortrait _portrait;

        public ElectrostaticProblem(List<Material> materials, IRightPart source)
        {
            _materials = materials;
            _source = source;
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleElectrostatics(mesh, _portrait, _source);
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            return new CGSolver().Solve(_matrix, _rhs);
        }

        public void PostProcess(double[] solution)
        {
            // Например, визуализация распределения потенциала
        }
    }
}
