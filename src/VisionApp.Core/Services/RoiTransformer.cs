using VisionApp.Core.Models;

namespace VisionApp.Core.Services;

/* Chuyển vùng kiểm tra từ toạ độ ảnh chuẩn sang toạ độ ảnh chụp theo kết quả định vị.
   Biến đổi cứng (rigid): chỉ xoay và tịnh tiến, kích thước vùng giữ nguyên. */
public static class RoiTransformer
{
    public static RotatedRoi ToLiveImage(Roi roi, AlignmentResult alignment)
    {
        ArgumentNullException.ThrowIfNull(roi);
        ArgumentNullException.ThrowIfNull(alignment);

        var liveCenter = ToLiveImage(roi.Center, alignment);
        return new RotatedRoi(liveCenter, roi.Width, roi.Height, alignment.AngleDegrees);
    }

    public static Point2D ToLiveImage(Point2D point, AlignmentResult alignment)
    {
        ArgumentNullException.ThrowIfNull(alignment);

        var radians = double.DegreesToRadians(alignment.AngleDegrees);
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var dx = point.X - alignment.Pivot.X;
        var dy = point.Y - alignment.Pivot.Y;

        return new Point2D(
            alignment.Pivot.X + alignment.OffsetX + cos * dx + sin * dy,
            alignment.Pivot.Y + alignment.OffsetY - sin * dx + cos * dy);
    }
}
