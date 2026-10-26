namespace VisionApp.Application.Interfaces;

/* Nguồn ảnh: camera thật, hoặc thư mục ảnh khi chạy giả lập. Mỗi lần gọi chụp một ảnh
   và đóng gói vào IInspectionContext — người gọi sở hữu context và phải Dispose nó. */
public interface IImageSource
{
    Task<IInspectionContext> AcquireAsync(string serialNumber, CancellationToken cancellationToken);
}
