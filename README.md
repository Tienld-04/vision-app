# vision-app

Hệ thống **kiểm tra quang học tự động (AOI) cho bo mạch PCB**, viết bằng C# / .NET 10.

Project học tập hướng tới vị trí **Machine Vision Engineer**: kết nối camera và xử lý ảnh
với hệ thống quản lý sản xuất (MES) của nhà máy.

> 🚧 Đang xây lại từ đầu, **từng phần một, từ Core lên giao diện**. Xem mục **Tiến độ** bên dưới.

---

## Hệ thống sẽ làm gì

Một bo mạch đi qua nhiều bước kiểm tra độc lập. Kết quả từng bước được gộp thành **một** kết luận.

| Bước                    | Việc                                                                                        |
| ----------------------- | ------------------------------------------------------------------------------------------- |
| **Định vị** (Alignment) | Tìm vị trí và góc xoay của bo trước khi kiểm. Không định vị được → ERROR, không kết luận NG |
| **Kiểm linh kiện**      | Linh kiện có mặt không, đúng vị trí không, có lệch hay xoay sai không                       |
| **Kiểm ngoại quan**     | Phát hiện bất thường bề mặt — bản đầu giới hạn đúng một loại lỗi                            |
| **Nhận dạng**           | Đọc mã QR / barcode, gắn kết quả với mã sản phẩm và mã lô                                   |
| **Tổng hợp**            | Gộp kết quả các bước thành kết luận cuối cùng                                               |

| Kết luận   | Ý nghĩa                                                                     |
| ---------- | --------------------------------------------------------------------------- |
| **OK**     | Đạt                                                                         |
| **NG**     | Có lỗi đã được xác nhận                                                     |
| **REVIEW** | Vùng xám — người vận hành phải duyệt trước khi chốt                         |
| **ERROR**  | Sự cố kỹ thuật, không kết luận được — kiểm tra lại, **không phải hàng lỗi** |

Ngoài xử lý ảnh, hệ thống còn có: recipe riêng cho từng loại PCB, truy vết theo mã PCB / lô /
nguyên nhân NG, thống kê thời gian xử lý, và gửi kết quả lên MES.

---

## Kiến trúc

```
VisionApp.HMI ─────────────► VisionApp.Application ──────► VisionApp.Core
  (WinForms)                   (pipeline, interface)          (mô hình + luật)
      │                                 ▲                           ▲
      └──► VisionApp.Infrastructure ────┴───────────────────────────┘
             (OpenCvSharp, nguồn ảnh, MES client, SQLite)

MES giả lập (ASP.NET Core Web API)  ← process riêng, không project nào reference
```

- **Core không phụ thuộc vào bất cứ thứ gì.** Luật nghiệp vụ test được mà không cần camera, ảnh hay giao diện.
- **Infrastructure implement các interface do Application định nghĩa** (dependency inversion).
  Nhờ vậy đổi OpenCvSharp sang Cognex VisionPro, hay camera giả sang camera thật,
  mà không phải sửa tầng nghiệp vụ.

---

## Tiến độ

| #   | Phần                                                  | Tầng           |     |
| --- | ----------------------------------------------------- | -------------- | --- |
| 1   | Mô hình kết luận: OK / NG / REVIEW / ERROR + luật gộp | Core           | ✅  |
| 2   | Hình học: vùng kiểm tra (ROI) + kết quả định vị       | Core           | ⬜  |
| 3   | Recipe                                                | Core           | ⬜  |
| 4   | Interface + pipeline điều phối các bước               | Application    | ⬜  |
| 5   | Nguồn ảnh + bước định vị                              | Infrastructure | ⬜  |
| 6   | Bước kiểm linh kiện                                   | Infrastructure | ⬜  |
| 7   | Giao diện hiển thị ảnh và kết quả                     | HMI            | ⬜  |

**Sau đó:** luồng duyệt REVIEW, truy vết bằng SQLite, MES giả lập, kiểm ngoại quan,
đọc mã sản phẩm, tích hợp camera và PLC thật.

---

## Cấu trúc hiện tại

```
vision-app/
├── VisionApp.sln
├── src/
│   └── VisionApp.Core/
│       ├── Inspection/          mô hình kết luận + luật gộp
│       └── note.md              giải thích chi tiết project này
└── tests/
    └── VisionApp.Core.Tests/
        └── Inspection/
```

---

## Chạy

Cần **.NET SDK 10** — kiểm tra bằng `dotnet --version`, phải ra `10.x`.

```powershell
dotnet build
dotnet test
```

Mở bằng Rider: mở file **`VisionApp.sln`**, không mở thư mục.

---

## Công nghệ

| Thành phần  | Công nghệ                                                        |
| ----------- | ---------------------------------------------------------------- |
| Ngôn ngữ    | C# / .NET 10                                                     |
| Giao diện   | WinForms                                                         |
| Xử lý ảnh   | OpenCvSharp — sau này có thể thay bằng Cognex VisionPro / HALCON |
| Lưu trữ     | SQLite                                                           |
| MES giả lập | ASP.NET Core Web API                                             |
| Kiểm thử    | xUnit                                                            |
