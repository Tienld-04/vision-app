namespace VisionApp.Core.ValueObjects;

/* Vùng kiểm tra hình chữ nhật trên ẢNH CHUẨN, khai báo trong recipe. Đơn vị pixel,
   (X, Y) là góc trên bên trái. */
public sealed record Roi
{
    public Roi(double x, double y, double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double X { get; }

    public double Y { get; }

    public double Width { get; }

    public double Height { get; }

    public Point2D Center => new(X + Width / 2, Y + Height / 2);
}
