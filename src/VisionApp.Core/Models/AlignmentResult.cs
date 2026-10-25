namespace VisionApp.Core.Models;

/* Kết quả định vị bo: so với ảnh chuẩn, bo trên ảnh chụp xoay AngleDegrees quanh Pivot rồi dịch đi (OffsetX, OffsetY).
   Góc dương = xoay ngược chiều kim đồng hồ khi nhìn trên màn hình, cùng quy ước với OpenCV. */
public sealed record AlignmentResult
{
    public AlignmentResult(Point2D pivot, double offsetX, double offsetY, double angleDegrees, double score)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(score, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(score, 1);

        Pivot = pivot;
        OffsetX = offsetX;
        OffsetY = offsetY;
        AngleDegrees = angleDegrees;
        Score = score;
    }

    public Point2D Pivot { get; }

    public double OffsetX { get; }

    public double OffsetY { get; }

    public double AngleDegrees { get; }

    public double Score { get; }
}
