using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using ElectroMagSimulator.Core;
using ElectroMagSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ElectroMagSimulator.Views
{
    public partial class DrawingCanvas : UserControl
    {
        private double _scale = 40.0;
        private Point _pan = new(0, 0);
        private Point? _lastMousePos;
        private TextBlock? _coordDisplay;
        private IMesh? _mesh;
        private double[]? _solution;
        private List<(Point p0, Point p1, Point p2, Point p3)> _cachedElementPoints = new();
        private List<Point> _cachedBoundaryNodes = new();

        private bool _showAreaBorders = false;
        public bool IsProbeMode { get; set; }
        private List<Point> _probePoints = new();
        public MaterialViewModel? SelectedMaterial { get; set; }
        private bool _isMaterialPaintMode;
        public bool IsMaterialPaintMode
        {
            get => _isMaterialPaintMode;
            set
            {
                _isMaterialPaintMode = value;
                UpdateCursor();
            }
        }
        public DrawingCanvas()
        {
            InitializeComponent();

            this.AttachedToVisualTree += (_, _) =>
            {
                _coordDisplay = this.FindAncestorOfType<MainWindow>()?.FindControl<TextBlock>("CoordLabel");
            };

            this.AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
            this.AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
            this.AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            this.AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        }
        public void SetProbePoints(IEnumerable<Point> points)
        {
            _probePoints = points.ToList();
            InvalidateVisual();
        }

        public void SetShowAreaBorders(bool show)
        {
            _showAreaBorders = show;
            InvalidateVisual();
        }
        public void SetMesh(IMesh? mesh)
        {
            _mesh = mesh;
            _solution = null;
            _cachedElementPoints.Clear();
            _cachedBoundaryNodes.Clear();

            if (mesh != null)
            {
                foreach (var element in mesh.Elements)
                {
                    var nodes = element.NodeIds.Select(id => mesh.GetNode(id)).ToArray();
                    if (nodes.Length == 4)
                    {
                        _cachedElementPoints.Add((
                            new Point(nodes[0].X, nodes[0].Y),
                            new Point(nodes[1].X, nodes[1].Y),
                            new Point(nodes[2].X, nodes[2].Y),
                            new Point(nodes[3].X, nodes[3].Y)
                        ));
                    }
                }

                double minX = mesh.Nodes.Min(n => n.X);
                double maxX = mesh.Nodes.Max(n => n.X);
                double minY = mesh.Nodes.Min(n => n.Y);
                double maxY = mesh.Nodes.Max(n => n.Y);

                foreach (var node in mesh.Nodes)
                {
                    bool isBoundary = Math.Abs(node.X - minX) < 1e-6 ||
                                      Math.Abs(node.X - maxX) < 1e-6 ||
                                      Math.Abs(node.Y - minY) < 1e-6 ||
                                      Math.Abs(node.Y - maxY) < 1e-6;
                    if (isBoundary)
                        _cachedBoundaryNodes.Add(new Point(node.X, node.Y));
                }
            }

            InvalidateVisual();
        }

        public void SetSolution(double[] solution)
        {
            _solution = solution;
            InvalidateVisual();
        }
        public void UpdateCursor()
        {
            if (IsMaterialPaintMode)
                this.Cursor = new Cursor(StandardCursorType.Hand);
            else if (IsProbeMode)
                this.Cursor = new Cursor(StandardCursorType.Cross); 
            else
                this.Cursor = new Cursor(StandardCursorType.Arrow);

        }

        public event Action<IGridArea>? AreaClicked;
        public event Action<Point>? PointClicked;
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (_mesh != null && IsProbeMode)
            {
                var worldPoint = ScreenToWorld(pos);    
                PointClicked?.Invoke(worldPoint);
                return;
            }

            if (_mesh != null && IsMaterialPaintMode && SelectedMaterial != null && SelectedMaterial.AreaId >= 0)
            {
                var worldPoint = ScreenToWorld(pos);

                var area = _mesh.Areas.FirstOrDefault(a =>
                    worldPoint.X >= a.X0 && worldPoint.X <= a.X1 &&
                    worldPoint.Y >= a.Y0 && worldPoint.Y <= a.Y1);

                if (area != null)
                {
                    AreaClicked?.Invoke(area);
                    return; 
                }
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                _lastMousePos = pos;
        }


        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _lastMousePos = null;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (_lastMousePos is { } last && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                var delta = pos - last;
                _pan += delta;
                _lastMousePos = pos;
                InvalidateVisual();
            }

            var logical = ScreenToWorld(pos);
            _coordDisplay?.SetValue(TextBlock.TextProperty, $"X: {logical.X:F2}, Y: {logical.Y:F2}");
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            var worldBefore = ScreenToWorld(mousePos);

            double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;
            _scale = Math.Clamp(_scale * zoomFactor, 0.01, 10000);

            var worldAfter = ScreenToWorld(mousePos);
            var worldDelta = worldAfter - worldBefore;

            _pan += new Vector(worldDelta.X * _scale, -worldDelta.Y * _scale);
            InvalidateVisual();
        }

        private Point ScreenToWorld(Point screen)
        {
            double x = (screen.X - Bounds.Width / 2 - _pan.X) / _scale;
            double y = -(screen.Y - Bounds.Height / 2 - _pan.Y) / _scale;
            return new Point(x, y);
        }

        private Point WorldToScreen(Point world)
        {
            double x = world.X * _scale + Bounds.Width / 2 + _pan.X;
            double y = -world.Y * _scale + Bounds.Height / 2 + _pan.Y;
            return new Point(x, y);
        }

        private double CalculateNiceStep()
        {
            double[] steps = { 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50 };
            double target = 60 / _scale;
            return steps.FirstOrDefault(s => s >= target) != 0 ? steps.First(s => s >= target) : steps.Last();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var gridPen = new Pen(Brushes.LightGray, 0.5);
            var axisPen = new Pen(Brushes.Black, 2);
            var elementPen = new Pen(Brushes.Gray, 1);
            var boundaryNodeBrush = Brushes.Red;

            double step = CalculateNiceStep();

            var topLeft = ScreenToWorld(new Point(0, 0));
            var bottomRight = ScreenToWorld(new Point(Bounds.Width, Bounds.Height));
            var probeBrush = Brushes.White;
           
            for (double x = Math.Floor(topLeft.X / step) * step; x <= bottomRight.X; x += step)
            {
                var p = WorldToScreen(new Point(x, 0));
                context.DrawLine(gridPen, new Point(p.X, 0), new Point(p.X, Bounds.Height));
            }

            for (double y = Math.Floor(bottomRight.Y / step) * step; y <= topLeft.Y; y += step)
            {
                var p = WorldToScreen(new Point(0, y));
                context.DrawLine(gridPen, new Point(0, p.Y), new Point(Bounds.Width, p.Y));
            }

            var x0_1 = WorldToScreen(new Point(topLeft.X, 0));
            var x0_2 = WorldToScreen(new Point(bottomRight.X, 0));
            var y0_1 = WorldToScreen(new Point(0, topLeft.Y));
            var y0_2 = WorldToScreen(new Point(0, bottomRight.Y));

            context.DrawLine(axisPen, new Point(x0_1.X, x0_1.Y), new Point(x0_2.X, x0_2.Y));
            context.DrawLine(axisPen, new Point(y0_1.X, y0_1.Y), new Point(y0_2.X, y0_2.Y));

            if (_mesh != null)
            {
                int elemIndex = 0;
                foreach (var (p0w, p1w, p2w, p3w) in _cachedElementPoints)
                {
                    var p0 = WorldToScreen(p0w);
                    var p1 = WorldToScreen(p1w);
                    var p2 = WorldToScreen(p2w);
                    var p3 = WorldToScreen(p3w);

                    var element = _mesh.GetElement(elemIndex++);
                    ISolidColorBrush? brush = null;
                    var material = _mesh.GetMaterialForElement(element);

                    if (material != null)
                    {
                        try { brush = new SolidColorBrush(Color.Parse(material.Color)); }
                        catch { brush = Brushes.Gray; }
                    }


                    var geo = new StreamGeometry();
                    using (var ctx = geo.Open())
                    {
                        ctx.BeginFigure(p0, true);
                        ctx.LineTo(p1);
                        ctx.LineTo(p3);
                        ctx.LineTo(p2);
                        ctx.LineTo(p0);
                    }

                    context.DrawGeometry(brush, elementPen, geo);
                }

                foreach (var node in _cachedBoundaryNodes)
                    context.DrawEllipse(boundaryNodeBrush, null, WorldToScreen(node), 2.5, 2.5);

                if (_showAreaBorders)
                {
                    var areaPen = new Pen(Brushes.Black, 3);
                    foreach (var area in _mesh.Areas)
                    {
                        var p0 = WorldToScreen(new Point(area.X0, area.Y0));
                        var p1 = WorldToScreen(new Point(area.X1, area.Y0));
                        var p2 = WorldToScreen(new Point(area.X1, area.Y1));
                        var p3 = WorldToScreen(new Point(area.X0, area.Y1));

                        context.DrawLine(areaPen, p0, p1);
                        context.DrawLine(areaPen, p1, p2);
                        context.DrawLine(areaPen, p2, p3);
                        context.DrawLine(areaPen, p3, p0);
                    }
                }
            }
            foreach (var probe in _probePoints)
            {
                var screenPoint = WorldToScreen(probe);
                context.DrawEllipse(probeBrush, null, screenPoint, 8, 8);
            }
        }
    }
}
