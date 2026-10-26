namespace VisionApp.Core.ValueObjects;

/* Một điểm trên ảnh, đơn vị pixel. Gốc (0, 0) ở góc trên bên trái, trục Y hướng xuống dưới. */
public sealed record Point2D
{
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; }

    public double Y { get; }
}
