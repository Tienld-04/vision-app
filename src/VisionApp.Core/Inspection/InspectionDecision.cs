namespace VisionApp.Core.Inspection;

/* Kết luận cuối cùng cho một sản phẩm, sau khi đã tổng hợp kết quả mọi bước kiểm tra. */
public enum InspectionDecision
{
    Ok = 1,
    Ng = 2,
    Review = 3,
    Error = 4
}
