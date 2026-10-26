namespace VisionApp.Application.Interfaces;

/* "Hộp kín" chứa ảnh chụp của một sản phẩm. Tầng Application chỉ cầm và chuyền nó đi,
   không biết bên trong là ảnh OpenCV hay VisionPro — chỉ Infrastructure mới mở ra được. */
public interface IInspectionContext : IDisposable
{
    string SerialNumber { get; }
}
