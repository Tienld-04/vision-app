namespace VisionApp.Core.Models;

/* Vùng kiểm tra đã được đặt lên ẢNH CHỤP: có thể bị xoay nên mô tả bằng tâm, kích thước và góc
   (giống RotatedRect của OpenCV). Kích thước giữ nguyên so với Roi gốc. */
public sealed record RotatedRoi
{
    public RotatedRoi(Point2D center, double width, double height, double angleDegrees)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Center = center;
        Width = width;
        Height = height;
        AngleDegrees = angleDegrees;
    }

    public Point2D Center { get; }

    public double Width { get; }

    public double Height { get; }

    public double AngleDegrees { get; }
}
