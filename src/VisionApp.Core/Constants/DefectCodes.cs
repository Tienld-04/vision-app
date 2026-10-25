namespace VisionApp.Core.Constants;

/* Mã lỗi gửi lên MES khi sản phẩm NG hoặc cần REVIEW.
   Dùng chuỗi chứ không dùng enum vì danh mục mã lỗi do nhà máy và khách hàng quy định. */
public static class DefectCodes
{
    public const string MissingComponent = "MISSING_COMPONENT";

    public const string ComponentShifted = "COMPONENT_SHIFTED";

    public const string ComponentRotated = "COMPONENT_ROTATED";

    public const string Scratch = "SCRATCH";
}
