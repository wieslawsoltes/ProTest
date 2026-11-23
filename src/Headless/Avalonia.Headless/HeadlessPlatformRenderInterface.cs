using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;

namespace Avalonia.Headless
{
    internal class HeadlessPlatformRenderInterface : IPlatformRenderInterface, IPlatformRenderInterfaceContext
    {
        public static void Initialize()
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformRenderInterface>().ToConstant(new HeadlessPlatformRenderInterface())
                .Bind<IFontManagerImpl>().ToConstant(new HeadlessFontManagerStub());
        }

        public IPlatformRenderInterfaceContext CreateBackendContext(IPlatformGraphicsContext? graphicsContext) => this;

        public bool SupportsIndividualRoundRects => false;

        public AlphaFormat DefaultAlphaFormat => AlphaFormat.Premul;

        public PixelFormat DefaultPixelFormat => PixelFormat.Rgba8888;
        public bool IsSupportedBitmapPixelFormat(PixelFormat format) => true;
        public bool SupportsRegions => false;
        public IPlatformRenderInterfaceRegion CreateRegion() => throw new NotSupportedException();

        public IGeometryImpl CreateEllipseGeometry(Rect rect) => new HeadlessGeometryStub(rect);

        public IGeometryImpl CreateLineGeometry(Point p1, Point p2)
        {
            var tl = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            var br = new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
            return new HeadlessGeometryStub(new Rect(tl, br));
        }

        public IGeometryImpl CreateRectangleGeometry(Rect rect)
        {
            return new HeadlessGeometryStub(rect);
        }

        public IStreamGeometryBuilder CreateStreamGeometryBuilder(IGeometryImpl? source = null)
        {
            return source switch
            {
                null => new HeadlessStreamGeometryBuilder([]),
                HeadlessStreamGeometryStub geometry => new HeadlessStreamGeometryBuilder(geometry.Figures),
                _ => throw new ArgumentException("The source must be a headless stream geometry.", nameof(source))
            };
        }

        public IGeometryImpl CreateGeometryGroup(FillRule fillRule, IReadOnlyList<IGeometryImpl> children) =>
            new HeadlessGeometryStub(children.Count != 0 ?
                children.Select(c => c.Bounds).Aggregate((a, b) => a.Union(b)) :
                default);

        public IGeometryImpl CreateCombinedGeometry(GeometryCombineMode combineMode, IGeometryImpl g1, IGeometryImpl g2) 
            => new HeadlessGeometryStub(g1.Bounds.Union(g2.Bounds));

        public IRenderTarget CreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces) => new HeadlessRenderTarget();
        public IDrawingContextLayerImpl CreateOffscreenRenderTarget(PixelSize pixelSize, Vector scaling,
            bool enableTextAntialiasing) => 
            new HeadlessBitmapStub(pixelSize, scaling * 96);

        public bool IsLost => false;
        public IReadOnlyDictionary<Type, object> PublicFeatures { get; } = new Dictionary<Type, object>();
        public PixelSize? MaxOffscreenRenderTargetPixelSize => null;
        public object? TryGetFeature(Type featureType) => null;

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            return new HeadlessBitmapStub(size, dpi);
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat format, AlphaFormat alphaFormat)
        {
            return new HeadlessBitmapStub(size, dpi);
        }

        public IBitmapImpl LoadBitmap(string fileName)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToWidth(Stream stream, int width,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToHeight(Stream stream, int height,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(string fileName)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(Stream stream)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }

        public IBitmapImpl LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size, Vector dpi, int stride)
        {
            return new HeadlessBitmapStub(new Size(1, 1), new Vector(96, 96));
        }        

        public IBitmapImpl LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new HeadlessBitmapStub(new Size(width, width), new Vector(96, 96));
        }

        public IBitmapImpl LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new HeadlessBitmapStub(new Size(height, height), new Vector(96, 96));
        }

        public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new HeadlessBitmapStub(destinationSize, new Vector(96, 96));
        }

        public IGeometryImpl BuildGlyphRunGeometry(GlyphRun glyphRun)
        {
            return new HeadlessGeometryStub(glyphRun.Bounds);
        }

        public IGlyphRunImpl CreateGlyphRun(
            GlyphTypeface glyphTypeface, 
            double fontRenderingEmSize,
            IReadOnlyList<GlyphInfo> glyphInfos, 
            Point baselineOrigin)
        {
            return new HeadlessGlyphRunStub(glyphTypeface, fontRenderingEmSize, baselineOrigin);
        }

        internal class HeadlessGlyphRunStub : IGlyphRunImpl
        {
            public HeadlessGlyphRunStub(
                GlyphTypeface glyphTypeface,
                double fontRenderingEmSize,
                Point baselineOrigin)
            {
                GlyphTypeface = glyphTypeface;
                FontRenderingEmSize = fontRenderingEmSize;
                BaselineOrigin = baselineOrigin;
            }

            public Rect Bounds { get; }

            public Point BaselineOrigin { get; }

            public GlyphTypeface GlyphTypeface { get; }

            public double FontRenderingEmSize { get; }           

            public void Dispose()
            {
            }

            public IReadOnlyList<float> GetIntersections(float lowerBound, float upperBound)
                => Array.Empty<float>();
        }

        private class HeadlessGeometryStub : IGeometryImpl
        {
            public HeadlessGeometryStub(Rect bounds)
            {
                Bounds = bounds;
            }

            public Rect Bounds { get; }
            
            public double ContourLength { get; } = 0;
            
            public virtual bool FillContains(Point point) => Bounds.Contains(point);

            public Rect GetRenderBounds(IPen? pen)
            {
                if(pen is null)
                {
                    return Bounds;
                }

                return Bounds.Inflate(pen.Thickness / 2);
            }

            public IGeometryImpl GetWidenedGeometry(IPen pen) => this;

            public bool StrokeContains(IPen? pen, Point point)
            {
                return false;
            }

            public IGeometryImpl Intersect(IGeometryImpl geometry)
                => new HeadlessGeometryStub(geometry.Bounds.Intersect(Bounds));

            public IntersectionResult GetFillIntersectionResult(IGeometryImpl geometry)
            {
                // The lightweight renderer approximates geometry intersections with bounds.
                var otherBounds = geometry.Bounds;
                if (Bounds.Size == default || otherBounds.Size == default || !Bounds.Intersects(otherBounds))
                    return IntersectionResult.Empty;

                if (Bounds.Contains(otherBounds))
                    return IntersectionResult.FullyInside;

                if (otherBounds.Contains(Bounds))
                    return IntersectionResult.FullyContains;

                return IntersectionResult.Intersects;
            }

            public ITransformedGeometryImpl WithTransform(Matrix transform) =>
                new HeadlessTransformedGeometryStub(this, transform);

            public bool TryGetPointAtDistance(double distance, out Point point)
            {
                point = new Point();
                return false;
            }

            public bool TryGetPointAndTangentAtDistance(double distance, out Point point, out Point tangent)
            {
                point = new Point();
                tangent = new Point();
                return false;
            }

            public bool TryGetSegment(double startDistance, double stopDistance, bool startOnBeginFigure, [NotNullWhen(true)] out IGeometryImpl? segmentGeometry)
            {
                segmentGeometry = null;
                return false;
            }
        }

        private class HeadlessTransformedGeometryStub : HeadlessGeometryStub, ITransformedGeometryImpl
        {
            public HeadlessTransformedGeometryStub(IGeometryImpl b, Matrix transform) : this(Fix(b, transform))
            {

            }

            private static (IGeometryImpl, Matrix, Rect) Fix(IGeometryImpl b, Matrix transform)
            {
                if (b is HeadlessTransformedGeometryStub transformed)
                {
                    b = transformed.SourceGeometry;
                    transform = transformed.Transform * transform;
                }

                return (b, transform, b.Bounds.TransformToAABB(transform));
            }

            private HeadlessTransformedGeometryStub((IGeometryImpl b, Matrix transform, Rect bounds) fix) : base(fix.bounds)
            {
                SourceGeometry = fix.b;
                Transform = fix.transform;
            }


            public IGeometryImpl SourceGeometry { get; }
            public Matrix Transform { get; }
        }

        private readonly record struct HeadlessFigure(Point[] Points, bool IsFilled);

        private sealed class HeadlessStreamGeometryStub(HeadlessFigure[] figures)
            : HeadlessGeometryStub(CalculateBounds(figures))
        {
            public IReadOnlyList<HeadlessFigure> Figures => figures;

            public override bool FillContains(Point point)
            {
                foreach (var figure in figures)
                {
                    if (figure.IsFilled && FigureContains(figure.Points, point))
                        return true;
                }

                return false;
            }

            private static Rect CalculateBounds(HeadlessFigure[] figures)
            {
                if (figures.Length == 0)
                    return default;

                var left = double.MaxValue;
                var right = double.MinValue;
                var top = double.MaxValue;
                var bottom = double.MinValue;

                foreach (var figure in figures)
                {
                    foreach (var point in figure.Points)
                    {
                        left = Math.Min(point.X, left);
                        right = Math.Max(point.X, right);
                        top = Math.Min(point.Y, top);
                        bottom = Math.Max(point.Y, bottom);
                    }
                }

                return new Rect(new Point(left, top), new Point(right, bottom));
            }

            private static bool FigureContains(Point[] points, Point point)
            {
                // Use the existing point-in-triangle approximation within each figure.
                for (var i = 0; i < points.Length; i++)
                {
                    var a = points[i];
                    var b = points[(i + 1) % points.Length];
                    var c = points[(i + 2) % points.Length];

                    Vector v0 = c - a;
                    Vector v1 = b - a;
                    Vector v2 = point - a;

                    var dot00 = v0 * v0;
                    var dot01 = v0 * v1;
                    var dot02 = v0 * v2;
                    var dot11 = v1 * v1;
                    var dot12 = v1 * v2;

                    var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
                    var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                    var v = (dot00 * dot12 - dot01 * dot02) * invDenom;
                    if (u >= 0 && v >= 0 && u + v < 1)
                        return true;
                }

                return false;
            }
        }

        private sealed class HeadlessStreamGeometryBuilder(IReadOnlyList<HeadlessFigure> source) : IStreamGeometryBuilder
        {
            private readonly List<HeadlessFigure> _figures = new(source);
            private readonly List<Point> _points = [];
            private bool _isFilled;
            private bool _built;

            public IGeometryImpl ToGeometry()
            {
                if (_built)
                    throw new InvalidOperationException("The geometry has already been built.");

                EndFigure(false);
                _built = true;
                return new HeadlessStreamGeometryStub(_figures.ToArray());
            }

            public void BeginFigure(Point startPoint, bool isFilled = true)
            {
                EndFigure(false);
                _isFilled = isFilled;
                _points.Add(startPoint);
            }

            public void EndFigure(bool isClosed)
            {
                if (_points.Count == 0)
                    return;

                _figures.Add(new HeadlessFigure(_points.ToArray(), _isFilled));
                _points.Clear();
            }

            public void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked = true)
                => _points.Add(point);

            public void CubicBezierTo(Point controlPoint1, Point controlPoint2, Point endPoint, bool isStroked = true)
            {
                _points.Add(controlPoint1);
                _points.Add(controlPoint2);
                _points.Add(endPoint);
            }

            public void QuadraticBezierTo(Point controlPoint, Point endPoint, bool isStroked = true)
            {
                _points.Add(controlPoint);
                _points.Add(endPoint);
            }

            public void LineTo(Point point, bool isStroked = true) => _points.Add(point);

            public void SetFillRule(FillRule fillRule)
            {
            }

            public void Dispose()
            {
            }
        }

        private class HeadlessBitmapStub : IBitmapImpl, IDrawingContextLayerImpl, IWriteableBitmapImpl, IRenderTargetBitmapImpl
        {
            public Size Size { get; }

            public HeadlessBitmapStub(Size size, Vector dpi)
            {
                Size = size;
                Dpi = dpi;
                var pixel = Size * (Dpi / 96);
                PixelSize = new PixelSize(Math.Max(1, (int)pixel.Width), Math.Max(1, (int)pixel.Height));
            }

            public HeadlessBitmapStub(PixelSize size, Vector dpi)
            {
                PixelSize = size;
                Dpi = dpi;
                Size = PixelSize.ToSizeWithDpi(dpi);
            }

            public void Dispose()
            {

            }

            public IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
            {
                return new HeadlessDrawingContextStub();
            }

            public IDrawingContextImpl CreateDrawingContext()
            {
                return new HeadlessDrawingContextStub();
            }

            public RenderTargetProperties Properties => default;

            public bool IsCorrupted => false;

            public void Blit(IDrawingContextImpl context)
            {
                
            }

            public bool CanBlit => false;

            public Vector Dpi { get; }
            public PixelSize PixelSize { get; }
            public PixelFormat? Format => PixelFormat.Rgba8888;
            public AlphaFormat? AlphaFormat => Platform.AlphaFormat.Premul;
            public int Version { get; set; }

            public void Save(Stream stream, BitmapEncoderOptions options)
            {
            }

            public ILockedFramebuffer Lock()
            {
                Version++;
                var mem = Marshal.AllocHGlobal(PixelSize.Width * PixelSize.Height * 4);
                return new LockedFramebuffer(mem, PixelSize, PixelSize.Width * 4, Dpi, PixelFormat.Rgba8888,
                    Platform.AlphaFormat.Premul, () => Marshal.FreeHGlobal(mem));
            }
        }

        internal class HeadlessDrawingContextStub : IDrawingContextImpl
        {
            public void Dispose()
            {

            }

            public Matrix Transform { get; set; }

            public void Clear(Color color)
            {

            }

            public IDrawingContextLayerImpl CreateLayer(PixelSize size)
            {
                return new HeadlessBitmapStub(size, new Vector(96, 96));
            }

            public void PushClip(Rect clip)
            {

            }

            public void PushClip(IPlatformRenderInterfaceRegion region)
            {
                
            }

            public void PopClip()
            {

            }

            public void PushLayer(Rect bounds)
            {
            }

            public void PopLayer()
            {
            }

            public void PushOpacity(double opacity, Rect? rect)
            {

            }

            public void PopOpacity()
            {

            }

            public void PushOpacityMask(IBrush mask, Rect bounds)
            {

            }

            public void PopOpacityMask()
            {

            }

            public void PushGeometryClip(IGeometryImpl clip)
            {

            }

            public void PopGeometryClip()
            {

            }

            public object? GetFeature(Type t)
            {
                return null;
            }

            public void DrawLine(IPen? pen, Point p1, Point p2)
            {
            }

            public void DrawGeometry(IBrush? brush, IPen? pen, IGeometryImpl geometry)
            {
            }

            public void DrawBitmap(IBitmapImpl source, double opacity, Rect sourceRect, Rect destRect)
            {
                
            }

            public void DrawBitmap(IBitmapImpl source, IBrush opacityMask, Rect opacityMaskRect, Rect destRect)
            {
                
            }

            public void DrawRectangle(IBrush? brush, IPen? pen, RoundedRect rect, BoxShadows boxShadow = default)
            {
                
            }

            public void DrawRegion(IBrush? brush, IPen? pen, IPlatformRenderInterfaceRegion region)
            {
                
            }

            public void DrawEllipse(IBrush? brush, IPen? pen, Rect rect)
            {
            }

            public void DrawGlyphRun(IBrush? foreground, IGlyphRunImpl glyphRun)
            {
                
            }

            public void PushClip(RoundedRect clip)
            {
                
            }

            public void PushRenderOptions(RenderOptions renderOptions)
            {
                
            }

            public void PopRenderOptions()
            {
                
            }

            public void PushTextOptions(TextOptions textOptions)
            {
                // No-op in headless stub
            }

            public void PopTextOptions()
            {
                // No-op in headless stub
            }
        }

        private class HeadlessRenderTarget : IRenderTarget
        {
            public RenderTargetProperties Properties => default;

            public void Dispose()
            {

            }

            public IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
            {
                return new HeadlessDrawingContextStub();
            }

            public IDrawingContextImpl CreateDrawingContext(IRenderTarget.RenderTargetSceneInfo sceneInfo,
                out RenderTargetDrawingContextProperties properties)
            {
                properties = default;
                return new HeadlessDrawingContextStub();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
