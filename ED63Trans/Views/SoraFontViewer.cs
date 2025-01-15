#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

#endregion

namespace ED63Trans.Views;

public class SoraFontViewer : Control
{
    public SoraFontViewer()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataProperty)
            InvalidateMeasure();
        else if (change.Property == PixelSizeProperty) SetValue(DataProperty, null);
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Black, null, new Rect(Bounds.Size));
        var pen = new Pen(new SolidColorBrush(new Color(0x40,255,255,255)));
        context.DrawLine(pen, new Point(Bounds.Width / 2, 0), new Point(Bounds.Width / 2, Bounds.Height));
        if (Data == null || Data.Length == 0) return;
        using var draw = new CustomOperation(Data, PixelSize, IsHalfChar);
        context.Custom(draw);
    }

    public class CustomOperation(
        byte[] data,
        int pixelSize,
        bool isHalf
    ) : ICustomDrawOperation
    {
        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;
            var paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 1,
                IsAntialias = false
            };
            var width = isHalf ? pixelSize / 2 : pixelSize;
            var index = 0;
            for (var i = 0; i < pixelSize; i++)
                for (var j = 0; j < width; j++)
                {
                    if (index >= data.Length) break;
                    var a1 = (byte)((data[index] & 0x0f) * 0x11);
                    var a2 = (byte)((data[index] >> 4) * 0x11);
                    if (a2 > 0)
                    {
                        paint.Color = new SKColor(255, 0, 0, a2);
                        canvas.DrawPoint(j, i, paint);
                    }

                    if (a1 > 0)
                    {
                        paint.Color = new SKColor(255, 0, 0, a1);
                        canvas.DrawPoint(j + 1, i, paint);
                    }

                    j++;
                    index++;
                }
        }

        public Rect Bounds { get; }
    }

    #region Properties

    public static readonly StyledProperty<int> PixelSizeProperty = AvaloniaProperty.Register<SoraFontViewer, int>(
        nameof(PixelSize));

    public static readonly StyledProperty<byte[]?> DataProperty = AvaloniaProperty.Register<SoraFontViewer, byte[]?>(
        nameof(Data), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsHalfCharProperty = AvaloniaProperty.Register<SoraFontViewer, bool>(
        nameof(IsHalfChar));

    public bool IsHalfChar
    {
        get => GetValue(IsHalfCharProperty);
        set => SetValue(IsHalfCharProperty, value);
    }

    public byte[]? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public int PixelSize
    {
        get => GetValue(PixelSizeProperty);
        set => SetValue(PixelSizeProperty, value);
    }

    #endregion
}