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
        private IReadOnlyList<Material> _materials;

        private double[] _solutionA;

        public double[] MagneticPotential => _solutionA;

        public MagnetostaticProblem(IReadOnlyList<Material> materials)
        {
            _materials = materials;
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleMagnetostatics(mesh, _materials, _portrait);
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            _solutionA = new CGSolver().Solve(_matrix, _rhs);
            return _solutionA;
        }

        public IPostProcessor PostProcessor { get; private set; }

        public void PostProcess(double[] solution)
        {
            PostProcessor = new MagnetostaticPostProcessor();
            PostProcessor.LoadMesh(_mesh, solution);
        }
    }

}
