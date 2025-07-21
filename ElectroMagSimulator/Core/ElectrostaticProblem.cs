using System.Collections.Generic;

namespace ElectroMagSimulator.Core
{
    public class ElectrostaticProblem : IProblem
    {
        private MatrixAssembler _assembler;
        private SparseMatrix _matrix;
        private double[] _rhs;
        private IMesh _mesh;
        private MatrixPortraitBuilder.MatrixPortrait _portrait;

        private IReadOnlyList<Material> _materials;
        private IRightPart _source;

        private double[] _solutionPhi;

        public double[] ElectricPotential => _solutionPhi;

        public ElectrostaticProblem(IReadOnlyList<Material> materials, IRightPart source)
        {
            _materials = materials;
            _source = source;
        }

        public void Assemble(IMesh mesh)
        {
            _mesh = mesh;
            _portrait = new MatrixPortraitBuilder().Build(mesh);
            _assembler = new MatrixAssembler();
            _assembler.AssembleElectrostatics(mesh, _materials, _portrait, _source); 
            _matrix = _assembler.GetMatrix();
            _rhs = _assembler.GetRhs();
        }

        public double[] Solve()
        {
            _solutionPhi = new CGSolver().Solve(_matrix, _rhs);
            return _solutionPhi;
        }

        public IPostProcessor PostProcessor { get; private set; }

        public void PostProcess(double[] solution)
        {
            PostProcessor = new ElectrostaticPostProcessor();
            PostProcessor.LoadMesh(_mesh, solution);
        }
    }
}
