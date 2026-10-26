# VisionApp.Core

## Vai trò của project

Tầng **trong cùng** của kiến trúc: chứa mô hình nghiệp vụ và luật kiểm tra của trạm AOI.

**Không phụ thuộc vào bất cứ thứ gì** — `VisionApp.Core.csproj` không có `PackageReference`
nào, cũng không reference project nào. Core không biết camera, ảnh, OpenCV, MES hay giao diện là gì.

```
Ai được phụ thuộc vào Core          Core phụ thuộc vào ai
──────────────────────────          ─────────────────────
VisionApp.Core.Tests   (hiện có)     (không ai)
VisionApp.Application  (đã tạo, đang tạm dừng)
VisionApp.Infrastructure (sau này)
VisionApp.HMI          (sau này)
```

Lợi ích: toàn bộ luật nghiệp vụ test được trong vài mili-giây, không cần phần cứng, không cần ảnh.

## Nghiệp vụ hiện có

Tóm tắt Core **đã làm được gì** dưới góc nhìn nghiệp vụ của trạm AOI. Chi tiết kỹ thuật từng phần
nằm ở các mục phía dưới.

### Một lần kiểm tra bo mạch — Core đang lo phần nào

```
Một bo mạch tới trạm AOI
 │
 ├─ 1. Nạp recipe theo loại PCB               ✅ dữ liệu recipe   ✅ kiểm tra recipe hợp lệ
 ├─ 2. Chụp ảnh                               ⬜ (Infrastructure)
 ├─ 3. Định vị bo: tìm 2 dấu định vị          ⬜ đo (Infrastructure)   ⬜ luật chấp nhận (Core)
 ├─ 4. Đặt vùng kiểm tra đúng chỗ trên ảnh    ✅ chuyển toạ độ
 ├─ 5. Kiểm từng linh kiện: có / thiếu / lệch ⬜ đo (Infrastructure)   ⬜ luật phán (Core)
 ├─ 6. Gộp kết quả → OK / NG / REVIEW / ERROR ✅ luật gộp
 └─ 7. Gửi kết quả + mã lỗi lên MES           ✅ danh mục mã lỗi       ⬜ gửi đi (Infrastructure)
```

Nguyên tắc chia việc: **Infrastructure đo** (điểm khớp, độ lệch — cần OpenCV), **Core phán** (so với ngưỡng
để ra OK/NG — không cần ảnh). Các ô "luật" ở bước 3 và 5 là phần Core còn thiếu.

### Danh sách nghiệp vụ

| # | Nghiệp vụ | Làm được gì | Kiểu dữ liệu / hàm |
|---|---|---|---|
| 1 | **Ra kết luận cho sản phẩm** | Gộp kết quả nhiều bước thành **OK / NG / REVIEW / ERROR**. Ưu tiên: `Failed > Error > NeedsReview > Passed`. Chưa có bước nào thực sự chạy → **ERROR**, không bao giờ OK. Nhiều lỗi cùng lúc → lấy lỗi của bước đầu tiên làm lỗi chính, ghi thêm số lỗi phụ | `ResultAggregator`, `StepResult`, `InspectionSummary`, `InspectionDecision`, `StepStatus` |
| 2 | **Danh mục mã lỗi gửi MES** | 4 mã: `MISSING_COMPONENT`, `COMPONENT_SHIFTED`, `COMPONENT_ROTATED`, `SCRATCH`. Không định vị được bo là ERROR (sự cố kỹ thuật) nên **không có mã lỗi** | `DefectCodes` |
| 3 | **Đặt vùng kiểm tra lên ảnh chụp** | Từ vùng kiểm tra khai báo trên **ảnh chuẩn** + kết quả định vị (bo lệch bao nhiêu, xoay bao nhiêu độ quanh dấu định vị) → tính ra **vị trí thật** trên ảnh chụp. Ví dụ R12: (120, 80) → (125.51, 80.77) | `RoiTransformer`, `Roi`, `AlignmentResult`, `RotatedRoi`, `Point2D` |
| 4 | **Recipe theo từng loại PCB** | Mỗi loại PCB một bộ cấu hình, nhận diện bằng **mã model + số bản**: 2 dấu định vị và độ khớp tối thiểu; danh sách linh kiện kèm vùng kiểm tra; ngưỡng có/thiếu, **vùng xám** ra REVIEW, độ lệch và góc xoay tối đa cho phép | `Recipe`, `AlignmentSettings`, `ComponentSpec`, `ComponentInspectionSettings` |
| 5 | **Chặn cấu hình vô lý ngay lúc tạo** | Không tạo được object nếu: vùng xám tràn ra ngoài [0, 1] (sẽ không bao giờ ra OK hoặc không bao giờ ra NG), kích thước vùng ≤ 0, độ khớp ngoài [0, 1], thiếu mã model, mã linh kiện rỗng... | constructor của từng kiểu |
| 6 | **Kiểm tra recipe trước khi cho trạm chạy** | Trả về **danh sách mọi lỗi** một lượt: không có linh kiện nào, trùng mã linh kiện, 2 dấu định vị quá gần (< 100 px, không tính được góc), vùng kiểm tra nằm ngoài khung ảnh | `RecipeValidator`, `RecipeLimits` |
| 7 | **Nhận diện recipe** | Cùng mã model + cùng số bản = **cùng một recipe**, kể cả khi ngưỡng bên trong vừa được chỉnh | `Recipe` (so theo định danh) |
| 8 | **Câu chữ cho người dùng** | Mọi thông điệp hiển thị cho người vận hành (lý do OK/NG) và cho kỹ sư soạn recipe (lỗi recipe) gom về một chỗ, không viết thẳng trong code | `InspectionMessages`, `RecipeMessages` |

### Chưa có — cần cho các phần sau

| Nghiệp vụ còn thiếu | Cần cho |
|---|---|
| Luật chấp nhận kết quả định vị: độ khớp < `MinScore` → ERROR | bước định vị |
| Luật phán linh kiện: điểm khớp → có / thiếu / vùng xám; lệch hoặc xoay quá mức → NG | bước kiểm linh kiện |
| Tên các bước + thứ tự, bật/tắt từng bước trong recipe | pipeline (tầng Application) |
| Cấu hình và mã lỗi cho bước đọc mã sản phẩm, bước kiểm ngoại quan | khi làm hai bước đó |
| Bản ghi truy vết (SN, recipe nào, lúc nào, kết luận gì) | lưu lịch sử bằng SQLite |
| Người vận hành duyệt REVIEW → OK / NG | luồng duyệt REVIEW |

Mức kiểm chứng hiện tại: nghiệp vụ 1 có unit test; nghiệp vụ 2–7 mới kiểm bằng chương trình tạm ngoài repo.
Hai luật của nghiệp vụ 1 vẫn đang chờ chốt — xem mục **Đang chờ review** ở cuối file.

## Cấu trúc

Theo cấu trúc chuẩn của tầng **Domain trong Clean Architecture** (.NET): chia thư mục theo loại,
giống chia package bên Java. Tên project giữ là `VisionApp.Core`.

```
Constants/                    hằng số — không có logic
├── DefectCodes.cs            mã lỗi gửi lên MES
├── InspectionMessages.cs     câu chữ hiển thị cho người vận hành
├── RecipeLimits.cs           giới hạn khi kiểm tra recipe
└── RecipeMessages.cs         thông báo lỗi recipe
Enums/
├── InspectionDecision.cs     kết luận cuối cùng của sản phẩm
└── StepStatus.cs             kết quả của một bước
Entities/                     đối tượng có định danh
└── Recipe.cs                 cấu hình kiểm tra cho một loại PCB — định danh: ModelCode + Version
ValueObjects/                 giá trị bất biến, so sánh theo nội dung
├── StepResult.cs             kết quả chi tiết của một bước
├── InspectionSummary.cs      kết quả tổng hợp
├── Point2D.cs                một điểm trên ảnh
├── Roi.cs                    vùng kiểm tra trên ảnh chuẩn
├── AlignmentResult.cs        kết quả định vị bo
├── RotatedRoi.cs             vùng kiểm tra đã đặt lên ảnh chụp
├── AlignmentSettings.cs      phần cấu hình của bước định vị
├── ComponentSpec.cs          một linh kiện cần kiểm
└── ComponentInspectionSettings.cs  ngưỡng của bước kiểm linh kiện
Services/                     luật nghiệp vụ — hàm thuần
├── ResultAggregator.cs       gộp nhiều bước thành một kết luận
├── RoiTransformer.cs         chuyển vùng kiểm tra từ ảnh chuẩn sang ảnh chụp
└── RecipeValidator.cs        kiểm tra recipe hợp lệ, trả về danh sách lỗi
```

Namespace đi theo đường dẫn thư mục — quy ước bắt buộc của .NET, giống package bên Java:

| Thư mục | Namespace | Java tương đương |
|---|---|---|
| `Constants/` | `VisionApp.Core.Constants` | package `constant` |
| `Enums/` | `VisionApp.Core.Enums` | package `enums` |
| `Entities/` | `VisionApp.Core.Entities` | package `entity` |
| `ValueObjects/` | `VisionApp.Core.ValueObjects` | package `model` / `vo` |
| `Services/` | `VisionApp.Core.Services` | package `service` |

> `Services/` ở đây là **hàm thuần `static`**, không phải bean `@Service` của Spring:
> không có trạng thái, không inject qua DI, gọi thẳng `ResultAggregator.Aggregate(...)`.

**Entity khác Value Object ở đâu?**

| | Entity | Value Object |
|---|---|---|
| Nhận diện bằng | **định danh** — `Recipe` nhận diện bằng `ModelCode` + `Version` | **toàn bộ nội dung** — hai `Roi` cùng toạ độ, cùng kích thước là một |
| Thay đổi nội dung | vẫn là "cùng một" đối tượng (sửa recipe PCB-A bản 1 thì nó vẫn là PCB-A bản 1) | đổi một giá trị là thành giá trị khác |
| Java tương đương | class `@Entity` có `@Id` bên JPA | class bất biến không có `@Id`, kiểu `@Embeddable` |

Thư mục `Exceptions/`, `Events/` của cấu trúc chuẩn **chưa tạo** — sẽ xuất hiện khi có file đầu tiên cần đến.

### So sánh bằng nhau — Entity dùng `class`, Value Object dùng `record`

Quy tắc: **thấy `class` là Entity, thấy `record` là Value Object.**

| | `class` | `record` |
|---|---|---|
| `a == b` | so **địa chỉ** bộ nhớ (có phải cùng một object không) | so **nội dung** (các field có bằng nhau không) |
| `Equals`, `GetHashCode`, `ToString` | phải tự viết (`ToString` mặc định chỉ ra tên kiểu) | compiler tự sinh — `ToString()` in cả nội dung |
| `with { ... }` | không có | có |
| Hợp với | **Entity** — so theo định danh | **Value Object** — so theo nội dung |

`sealed` (cả hai loại) = **không cho kế thừa**, tương đương `final class` bên Java.
Các kiểu này không thiết kế để kế thừa — lớp con thêm field sẽ làm sai phép so sánh.

**Value Object → `sealed record`.** So sánh theo nội dung là bản chất của Value Object, và `record` tự làm sẵn:

```csharp
new Roi(10, 20, 30, 40) == new Roi(10, 20, 30, 40)    // true
new Roi(10, 20, 30, 40).ToString()
// "Roi { X = 10, Y = 20, Width = 30, Height = 40, Center = Point2D { X = 25, Y = 40 } }"
```

- **So cả kiểu:** một `Roi` và một `RotatedRoi` có cùng các con số vẫn **không** bằng nhau.
- **So sâu:** `ComponentSpec` chứa `Roi` → so `ComponentSpec` thì tự so luôn `Roi` bên trong.
- **Vẫn giữ kiểu viết constructor + property chỉ `get`** (không dùng cú pháp `record Roi(double X, ...)`
  rút gọn), để constructor kiểm được ràng buộc và `with` không lách qua được ràng buộc đó.

**Ngoại lệ — Value Object chứa danh sách phải tự viết `Equals`:** `record` so danh sách theo **địa chỉ**,
nên hai `InspectionSummary` có `Steps` giống hệt nhau vẫn bị coi là khác. Vì vậy `InspectionSummary`
tự viết `Equals` (so `Steps` bằng `SequenceEqual` — từng phần tử một) và `GetHashCode`
(bắt buộc viết kèm, thiếu là compiler cảnh báo CS8851).

**Entity (`Recipe`) → `sealed class`, so theo định danh.** Tự viết `Equals`, `GetHashCode`, `==`, `!=`
dựa trên `ModelCode` + `Version`, **bỏ qua** nội dung cấu hình:

```
PCB-A bản 1, ngưỡng 0.70   ==   PCB-A bản 1, ngưỡng 0.75     → true  (cùng một recipe, vừa chỉnh ngưỡng)
PCB-A bản 1                !=   PCB-A bản 2                  → true
PCB-A bản 1                !=   pcb-a bản 1                  → true  (ModelCode so CHÍNH XÁC, phân biệt hoa thường)
```

`ModelCode` so chính xác (`StringComparison.Ordinal`) vì nó phải khớp đúng mã mà MES trả về.

**Bẫy cần nhớ khi phỏng vấn:** với `class`, nếu chỉ override `Equals` mà quên `==` thì `a == b` **vẫn so
địa chỉ** còn `a.Equals(b)` lại so định danh → hai kết quả khác nhau. Vì vậy `Recipe` overload cả `==` và `!=`.

**Đối chiếu Java:** đúng hợp đồng `equals()` / `hashCode()` bên Java — override `equals` thì bắt buộc
override `hashCode`. Khác ở chỗ Java **không cho** overload `==` (luôn so tham chiếu), còn C# cho phép.
`record` của C# giống `record` của Java 16 (tự sinh `equals`/`hashCode`/`toString`), thêm cú pháp `with`.

Test đặt cùng cấu trúc thư mục bên `tests/VisionApp.Core.Tests/` (`Services/`, `ValueObjects/`)
để tìm test của một file là thấy ngay.

---

## Mô hình kết luận

Một sản phẩm đi qua nhiều bước kiểm tra. Mỗi bước cho ra một `StepResult`.
`ResultAggregator` gộp chúng thành **một** `InspectionSummary`.

```
Pipeline (tầng Application — chưa làm)
  ├─ Alignment          → StepResult ┐
  ├─ ComponentPresence  → StepResult ├─► ResultAggregator.Aggregate() ─► InspectionSummary ─► HMI, MES
  └─ SurfaceDefect      → StepResult ┘
```

### InspectionDecision — kết luận cuối cùng

| Giá trị | Ý nghĩa |
|---|---|
| `Ok` | Đạt — sản phẩm được đi tiếp |
| `Ng` | Không đạt — có ít nhất một lỗi đã được xác nhận |
| `Review` | Vùng xám — người vận hành phải xem lại trước khi chốt OK hay NG |
| `Error` | Không kết luận được vì sự cố kỹ thuật (mất ảnh, không định vị được bo, bước bị treo...) |

> `Error` **không phải hàng lỗi**. Sản phẩm không hỏng — chỉ là trạm chưa kiểm tra được.
> Hàng `Error` phải được kiểm tra lại, không được loại bỏ.

### StepStatus — kết quả của một bước

| Giá trị | Ý nghĩa |
|---|---|
| `Passed` | Bước này không phát hiện vấn đề gì |
| `Failed` | Phát hiện lỗi chắc chắn |
| `NeedsReview` | Thấy nghi ngờ nhưng không đủ chắc để kết luận |
| `Error` | Không chạy được tới kết luận vì sự cố kỹ thuật |
| `Skipped` | Không chạy — bị tắt trong recipe, hoặc bị bỏ qua vì bước trước đã hỏng |

Tách riêng với `InspectionDecision` vì bước có trạng thái `Skipped`, còn sản phẩm thì không.

### StepResult — kết quả chi tiết của một bước

| Field | Ý nghĩa |
|---|---|
| `StepName` | Tên bước, ví dụ `"Alignment"`, `"ComponentPresence"` |
| `Status` | Một giá trị của `StepStatus` |
| `DefectCode` | Mã lỗi gửi lên MES. **Bắt buộc** khi `Failed`, tuỳ chọn khi `NeedsReview`, còn lại là `null` |
| `Message` | Diễn giải cho người vận hành đọc trên HMI |
| `Duration` | Thời gian bước chạy — do **pipeline** gán, bước không tự đo |

**Chỉ tạo được qua factory method:**

```csharp
StepResult.Passed("Alignment")
StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12")
StepResult.NeedsReview("ComponentPresence", "C5 mờ", suspectedDefectCode: DefectCodes.MissingComponent)
StepResult.Error("SurfaceDefect", "Hết thời gian xử lý")
StepResult.Skipped("SurfaceDefect", "Tắt trong recipe")
```

Constructor là `private`, nên bên ngoài không thể `new StepResult(...)`. Gọi `Failed` mà không có mã lỗi
→ văng `ArgumentException` ngay lúc tạo.

Truyền `message` là `null` cũng văng `ArgumentNullException`.

**Bảo đảm bằng compiler, không phải bằng kiểm tra lúc chạy:**

`with` là cú pháp tạo bản copy của record với một vài field đổi giá trị. Bốn field đầu chỉ có `get`,
nên `with` không sửa được chúng — đã kiểm chứng:

```csharp
StepResult.Passed("x") with { Status = StepStatus.Failed };
// error CS0200: Property 'StepResult.Status' cannot be assigned to -- it is read only
```

Chỉ `Duration` có `init`, nên chỉ nó sửa được bằng `with` — đúng nhu cầu của pipeline:

```csharp
results.Add(result with { Duration = stopwatch.Elapsed });   // result gốc vẫn có Duration = 0
```

**Đối chiếu Java:**

| C# | Java |
|---|---|
| `record` | `record` (Java 16+) |
| `with { X = ... }` | chưa có sẵn — phải tự viết method `withX(...)` |
| factory method `static` | static factory method, giống hệt |
| `{ get; }` | field `final` + getter |
| `{ get; init; }` | không có tương đương — gán được lúc tạo / lúc `with`, sau đó khoá |

### InspectionSummary — kết quả tổng hợp

| Field | Ý nghĩa |
|---|---|
| `Decision` | Kết luận cuối cùng |
| `Reason` | Lý do, dạng `"TênBước: thông điệp"`, hiển thị trên HMI |
| `DefectCode` | Mã lỗi chính. Có khi `Ng`, có thể có khi `Review` |
| `Steps` | Kết quả từng bước, giữ nguyên thứ tự chạy — phục vụ truy vết |

Tạo qua constructor như mọi Value Object khác: `Reason`, `Steps` và từng phần tử trong `Steps` không được
`null`; `Steps` được **copy ra mảng riêng** ngay trong constructor — người gọi sửa list của họ sau đó thì
summary vẫn giữ nguyên.

So sánh hai summary thì so **từng bước một** trong `Steps` (tự viết `Equals` — xem mục "So sánh bằng nhau").

### ResultAggregator — luật gộp

Hàm thuần (`static`): không I/O, không trạng thái, cùng đầu vào luôn cho cùng đầu ra.
Tương đương một utility class toàn method `static` bên Java.

Kiểm tra **lần lượt từ trên xuống**, gặp điều kiện đúng đầu tiên thì dừng:

| # | Điều kiện | Kết luận | Vì sao |
|---|---|---|---|
| 1 | Có bước `Failed` | **NG** | Lỗi đã chắc chắn → bo hỏng, dù bước khác chưa chạy xong cũng không cứu được |
| 2 | Có bước `Error` | **ERROR** | Không được đưa người vận hành duyệt một bo chưa được kiểm tra đầy đủ |
| 3 | Có bước `NeedsReview` | **REVIEW** | |
| 4 | Không có bước nào `Passed` (danh sách rỗng, hoặc toàn `Skipped`) | **ERROR** | Chống "đạt cho có" — chưa kiểm gì mà ra OK là cho hàng chưa kiểm đi tiếp |
| 5 | Còn lại | **OK** | |

Chi tiết:
- Nhiều bước cùng `Failed` → lấy **bước đầu tiên** làm lỗi chính, lý do ghi thêm `"(+N lỗi khác)"`.
- `Passed` + `Skipped` → **OK**. Tắt một bước trong recipe là quyết định có chủ đích, không phải sự cố.
- Danh sách `Steps` được **copy ra mảng riêng**. Người gọi có sửa `List` của họ sau đó thì summary vẫn giữ nguyên.

### DefectCodes — mã lỗi

Mã lỗi gửi lên MES khi sản phẩm NG, hoặc kèm theo khi cần REVIEW. MES dùng mã này để
thống kê lỗi theo loại, theo ca, theo lô.

| Hằng số | Giá trị | Bước sinh ra |
|---|---|---|
| `MissingComponent` | `"MISSING_COMPONENT"` | kiểm linh kiện — thiếu linh kiện |
| `ComponentShifted` | `"COMPONENT_SHIFTED"` | kiểm linh kiện — lệch vị trí quá giới hạn |
| `ComponentRotated` | `"COMPONENT_ROTATED"` | kiểm linh kiện — xoay sai góc |
| `Scratch` | `"SCRATCH"` | kiểm ngoại quan — vết xước |

**Vì sao chuỗi mà không dùng enum?** Danh mục mã lỗi do **nhà máy và khách hàng** quy định,
và MES nhận mã dạng chuỗi. Đổi hay thêm mã chỉ là sửa giá trị chuỗi, không ảnh hưởng tới kiểu dữ liệu
ở những nơi đang dùng.

**Không có mã lỗi cho bước định vị.** Không định vị được bo là **ERROR** (sự cố kỹ thuật),
không phải NG — nên không gửi mã lỗi nào lên MES.

Mã của bước đọc mã sản phẩm sẽ thêm khi làm tới bước đó.

### InspectionMessages — câu chữ hiển thị

Mọi câu chữ mà Core sinh ra để người vận hành đọc đều nằm ở đây, **không viết thẳng trong code**.
Đổi câu chữ thì sửa đúng một file.

| Hằng số | Nội dung | Ai dùng |
|---|---|---|
| `StepPassed` | `"Đạt"` | message mặc định của `StepResult.Passed()` |
| `AllStepsPassed` | `"Đạt tất cả các bước kiểm tra"` | lý do khi kết luận OK |
| `NoStepExecuted` | `"Không có bước kiểm tra nào được thực hiện"` | lý do khi không có bước nào chạy |
| `StepReasonFormat` | `"{0}: {1}"` | ghép tên bước + thông điệp của bước |
| `AdditionalFailuresFormat` | `"{0} (+{1} lỗi khác)"` | thêm số lỗi phụ khi nhiều bước cùng Failed |

**Quy ước:** hằng số có hậu tố `Format` là **chuỗi mẫu**, phải dùng với `string.Format`:

```csharp
string.Format(InspectionMessages.StepReasonFormat, "ComponentPresence", "Thiếu R12")
// → "ComponentPresence: Thiếu R12"
```

Lý do dùng chuỗi mẫu `{0}` thay vì `$"..."`: chuỗi `$"{x}"` (interpolated string) được ghép
**ngay tại chỗ viết**, nên không cất vào hằng số được. Chuỗi mẫu thì cất được, ghép sau.

`const` dùng được làm **giá trị mặc định của tham số** — điều mà `static readonly` không làm được:

```csharp
public static StepResult Passed(string stepName, string message = InspectionMessages.StepPassed)
```

**Vì sao hằng số mà không đọc từ file config (appsettings.json)?**

1. **Core không phụ thuộc gì.** Đọc file config cần package `Microsoft.Extensions.Configuration`
   — thêm vào là phá vỡ nguyên tắc số một của tầng này.
2. **Config dành cho thông số vận hành** mà kỹ sư chỉnh theo từng nhà máy, từng trạm: ngưỡng đo,
   mã trạm, timeout. Câu chữ hiển thị không thuộc loại đó.

**Khi nào chuyển sang file resource (`.resx`)?** Khi giao diện cần **nhiều ngôn ngữ**
(ví dụ tiếng Việt cho người vận hành, tiếng Anh cho kỹ sư của khách hàng).
`.resx` là cách chuẩn của .NET — tương đương `messages.properties` + `MessageSource` bên Spring,
và không cần package nào nên vẫn đặt được trong Core. Các hằng số `Format` ở trên chuyển sang
`.resx` được nguyên văn.

---

## Nơi khác gọi đến như nào

**Hiện tại** — người gọi duy nhất là test:

```csharp
using VisionApp.Core.Constants;   // DefectCodes, InspectionMessages
using VisionApp.Core.Enums;       // InspectionDecision
using VisionApp.Core.ValueObjects; // StepResult
using VisionApp.Core.Services;    // ResultAggregator

var summary = ResultAggregator.Aggregate(
[
    StepResult.Passed("Alignment"),
    StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12")
]);

// summary.Decision   == InspectionDecision.Ng
// summary.DefectCode == DefectCodes.MissingComponent
// summary.Reason     == "ComponentPresence: Thiếu R12"
```

**Sau này** — pipeline ở tầng Application *(minh hoạ, chưa có code)*:

```csharp
var results = new List<StepResult>();
foreach (var step in steps)
{
    var stopwatch = Stopwatch.StartNew();
    var result = await step.ExecuteAsync(context, ct);
    results.Add(result with { Duration = stopwatch.Elapsed });
}
var summary = ResultAggregator.Aggregate(results);
```

- **HMI** đọc `summary.Decision` để tô màu, `summary.Steps` để hiện bảng chi tiết.
- **MES client** đọc `Decision` + `DefectCode` để báo kết quả.

---

## Hình học — đặt vùng kiểm tra lên ảnh chụp

### Vai trò

Recipe khai báo vị trí từng linh kiện trên **ảnh chuẩn** (ảnh của một bo mẫu đặt ngay ngắn).
Nhưng bo thật đặt lên băng tải luôn lệch và xoay một chút:

```
Recipe:  R12 nằm ở (120, 80) trên ảnh chuẩn
Bo thật: lệch 5px sang phải, 2px xuống dưới, xoay 1°
  → R12 thật ra đang ở (125.51, 80.77) trên ảnh chụp

Kiểm ngay tại (120, 80)      → soi nhầm chỗ → báo NG oan
Chuyển toạ độ trước rồi kiểm → soi đúng R12
```

Đây là nguyên tắc cốt lõi của AOI: **định vị trước, kiểm tra sau**.

### Hệ toạ độ

- Đơn vị **pixel**. Gốc `(0, 0)` ở **góc trên bên trái**, trục Y **hướng xuống dưới** (chuẩn của mọi thư viện ảnh).
- Góc **dương = xoay ngược chiều kim đồng hồ** khi nhìn trên màn hình — cùng quy ước với OpenCV,
  để tầng Infrastructure dùng OpenCV không phải đổi dấu.

### Các kiểu dữ liệu

| Kiểu | Loại | Nội dung |
|---|---|---|
| `Point2D` | Value Object (`record`) | `X`, `Y` |
| `Roi` | Value Object (`record`) | `X`, `Y` (góc trên trái), `Width`, `Height`, `Center` (tính ra) — trên **ảnh chuẩn** |
| `AlignmentResult` | Value Object (`record`) | `Pivot`, `OffsetX`, `OffsetY`, `AngleDegrees`, `Score` (độ tin cậy 0..1) |
| `RotatedRoi` | Value Object (`record`) | `Center`, `Width`, `Height`, `AngleDegrees` — trên **ảnh chụp**, giống `RotatedRect` của OpenCV |
| `RoiTransformer` | `static class` | phép chuyển toạ độ |

Ràng buộc kiểm ngay trong constructor: `Width`, `Height` phải > 0; `Score` phải nằm trong `[0, 1]`;
`Pivot`, `Center` không được `null`. Sai thì văng exception lúc tạo object.

**Vì sao `Point2D` là `record` thường mà không phải `record struct`?**

C# có thêm `struct` (value type) — thứ Java chưa có. `record` cũng viết được dạng `record struct`:

| | `struct` (value type) | `record` / `class` (reference type) |
|---|---|---|
| Lưu ở đâu | nằm ngay trong biến chứa nó, không cấp phát heap | trên heap, biến chỉ giữ tham chiếu |
| Gán / truyền tham số | **copy** toàn bộ giá trị | copy tham chiếu |
| Có thể là `null`? | không | có — nên phải kiểm `null` trong constructor |
| Ép được ràng buộc? | **Không** — luôn tồn tại `default(T)` với mọi field = 0, bỏ qua constructor | Có — chỉ tạo được qua constructor |

- `Point2D` dùng `sealed record` (reference type) như mọi Value Object khác — **một kiểu viết duy nhất**, dễ đọc.
- Ở quy mô này (vài chục điểm mỗi chu kỳ) chi phí cấp phát heap là không đáng kể.
- `record struct` vẫn là lựa chọn đúng cho code chạy **trên từng pixel** (hàng triệu lần mỗi ảnh) — khi nào
  có code như vậy mới cần cân nhắc lại.

Câu "struct vs class" rất hay bị hỏi khi phỏng vấn C#.

### Xử lý như nào

`AlignmentResult` mô tả: so với ảnh chuẩn, bo trên ảnh chụp đã **xoay `AngleDegrees` quanh điểm `Pivot`**,
rồi **dịch đi `(OffsetX, OffsetY)`**. `RoiTransformer` áp đúng phép đó lên tâm của vùng kiểm tra:

```
dx = x − Pivot.X                                  ← đưa điểm về hệ toạ độ lấy Pivot làm gốc
dy = y − Pivot.Y

x' = Pivot.X + OffsetX + cos θ · dx + sin θ · dy   ← xoay, đưa về chỗ cũ, rồi dịch
y' = Pivot.Y + OffsetY − sin θ · dx + cos θ · dy
```

- **Vì sao xoay quanh `Pivot` chứ không quanh gốc `(0, 0)`?** `Pivot` là dấu định vị (fiducial) trên bo.
  Khi đó `Offset` có nghĩa trực quan: "dấu định vị dịch đi bao nhiêu". Nếu xoay quanh góc ảnh thì
  chỉ 1° thôi cũng làm những điểm xa góc ảnh nhảy đi rất nhiều, `Offset` mất hết ý nghĩa.
- **Dấu `+ sin` / `− sin`** là do trục Y hướng xuống. Với cách viết này, góc dương là ngược chiều kim đồng hồ
  trên màn hình — đã đối chiếu, trùng khớp với công thức `getRotationMatrix2D` của OpenCV.
- Đây là **biến đổi cứng (rigid)**: chỉ xoay và dịch, **không co giãn**, nên `Width`, `Height` giữ nguyên.
- `RoiTransformer.ToLiveImage` có **2 overload** (cùng tên, khác tham số — giống overload bên Java):
  một cái nhận `Point2D`, một cái nhận `Roi`.

### Nơi khác gọi đến như nào

**Hiện tại** — chưa có nơi nào gọi. Cách gọi:

```csharp
using VisionApp.Core.ValueObjects;
using VisionApp.Core.Services;

var r12 = new Roi(x: 100, y: 60, width: 40, height: 40);       // tâm (120, 80) trên ảnh chuẩn

var alignment = new AlignmentResult(
    pivot: new Point2D(50, 50), offsetX: 5, offsetY: 2, angleDegrees: 1, score: 0.97);

RotatedRoi placed = RoiTransformer.ToLiveImage(r12, alignment);
// placed.Center ≈ (125.51, 80.77), kích thước 40×40, góc 1°
```

**Sau này** *(minh hoạ, chưa có code)*:

- **Bước định vị** (Infrastructure, dùng OpenCV) **tạo ra** `AlignmentResult`: tìm 2 dấu định vị trên ảnh chụp.
  `Pivot` = vị trí dấu thứ nhất trên ảnh chuẩn; `Offset` = dấu thứ nhất dịch đi bao nhiêu;
  `AngleDegrees` = góc của đoạn nối hai dấu thay đổi bao nhiêu; `Score` = độ khớp khi tìm dấu.
- **Bước kiểm linh kiện** (Infrastructure) **dùng** nó cho từng linh kiện trong recipe:

  ```csharp
  foreach (var component in recipe.Components)
  {
      var liveRoi = RoiTransformer.ToLiveImage(component.Roi, alignment);
      // cắt vùng liveRoi trên ảnh chụp rồi kiểm: có / thiếu / lệch
  }
  ```

- **HMI** vẽ các `RotatedRoi` lên ảnh làm khung overlay xanh/đỏ.
- `Score` sẽ được so với ngưỡng trong recipe (phần 3) để quyết định định vị đủ tin cậy hay phải báo ERROR.

### Kiểm chứng

Phần này **chưa có unit test** (thư mục `tests/` tạm chưa động tới). Đã kiểm bằng một chương trình tạm
đặt ngoài repo — 12/12 đạt:

- không lệch không xoay → điểm giữ nguyên
- chỉ tịnh tiến (5, 2) → điểm dịch đúng (5, 2)
- xoay +90° → điểm bên phải `Pivot` chuyển lên **trên** `Pivot` (đúng chiều ngược kim đồng hồ trên màn hình)
- bản thân `Pivot` chỉ bị dịch, không bị ảnh hưởng bởi góc xoay
- trùng khớp công thức `getRotationMatrix2D` của OpenCV
- kích thước vùng giữ nguyên sau khi chuyển
- `Width = 0`, `Height < 0`, `Score > 1`, `Score < 0` đều văng exception
- hai `Point2D` cùng toạ độ thì bằng nhau (so sánh theo giá trị)

---

## Recipe — cấu hình kiểm tra cho một loại PCB

### Vai trò

Mỗi loại PCB có một recipe riêng. Đổi đơn hàng trên dây chuyền = nạp recipe khác, **không sửa code**.
Recipe trả lời: kiểm bo này thì định vị bằng dấu nào, kiểm những linh kiện nào ở đâu, ngưỡng bao nhiêu.

```
Recipe  (ModelCode "PCB-A", Version 1)
├── Alignment : AlignmentSettings              ← bước định vị đọc phần này
│   ├── Fiducial1 : Roi    dấu định vị thứ nhất trên ảnh chuẩn
│   ├── Fiducial2 : Roi    dấu định vị thứ hai
│   ├── MinScore           độ khớp tối thiểu khi tìm dấu
│   └── Pivot              (tính ra) = tâm Fiducial1
├── Components : ComponentSpec[]               ← bước kiểm linh kiện đọc phần này
│   ├── { Designator "R12", Roi (100, 60, 40×40) }
│   └── { Designator "C5",  Roi (200, 120, 30×20) }
└── ComponentInspection : ComponentInspectionSettings
    ├── PresenceThreshold  ngưỡng có/thiếu
    ├── ReviewMargin       độ rộng vùng xám
    ├── MaxShiftPx         lệch tối đa cho phép (pixel)
    └── MaxRotationDegrees xoay tối đa cho phép (độ)
```

Chia nhóm **theo bước kiểm tra**: sau này mỗi bước chỉ nhận đúng phần cấu hình của nó
(bước định vị nhận `AlignmentSettings`, không cần biết danh sách linh kiện).

`ModelCode` + `Version` dùng để truy vết: kết quả kiểm tra ghi lại "kiểm bằng recipe nào, bản thứ mấy",
nên sửa recipe thì tăng `Version`.

### Xử lý như nào

**Vùng xám — nơi sinh ra kết luận REVIEW ở phần 1:**

Bước kiểm linh kiện so vùng ảnh chụp với ảnh chuẩn, ra một **điểm khớp** từ 0 đến 1.
`PresenceThreshold` và `ReviewMargin` chia điểm đó làm 3 vùng:

```
 0 ───────── T−M ═════════ T+M ───────── 1
    Failed       NeedsReview      Passed
   (thiếu)       (vùng xám)      (có mặt)

Ví dụ T = 0.7, M = 0.1:
  điểm ≥ 0.8        → Passed
  điểm < 0.6        → Failed (MISSING_COMPONENT)
  0.6 ≤ điểm < 0.8  → NeedsReview → kết luận REVIEW, người vận hành duyệt
```

`M = 0` nghĩa là không có vùng xám: trên ngưỡng là có, dưới ngưỡng là thiếu.

Hàm phân loại điểm vào 3 vùng **chưa viết** — sẽ làm cùng bước kiểm linh kiện.
Ở phần này mới có dữ liệu cấu hình và ràng buộc của nó.

**Ràng buộc kiểm ngay trong constructor** (sai thì văng `ArgumentOutOfRangeException` / `ArgumentException`):

| Kiểu | Ràng buộc | Vì sao |
|---|---|---|
| `ComponentInspectionSettings` | `ReviewMargin ≥ 0` | |
| | `T − M ≥ 0` | nếu không, **không bao giờ ra Failed** → linh kiện thiếu lọt qua thành REVIEW / OK |
| | `T + M ≤ 1` | nếu không, **không bao giờ ra Passed** → mọi bo đều phải người duyệt |
| | `MaxShiftPx > 0`, `MaxRotationDegrees > 0` | |
| `AlignmentSettings` | `MinScore` trong `[0, 1]` | |
| `ComponentSpec` | `Designator` không rỗng | |
| `Recipe` | `ModelCode` không rỗng, `Version ≥ 1` | |
| | không phần tử nào trong `Components` là `null` | |

Hai ràng buộc vùng xám được viết bằng hàm có sẵn của .NET, không cần câu thông báo tự viết:

```csharp
ArgumentOutOfRangeException.ThrowIfLessThan(presenceThreshold, reviewMargin);          // T − M ≥ 0
ArgumentOutOfRangeException.ThrowIfGreaterThan(presenceThreshold, 1 - reviewMargin);   // T + M ≤ 1
```

**`Pivot` chỉ có ở một chỗ:** `AlignmentSettings.Pivot => Fiducial1.Center`.
Bước định vị sau này phải lấy `Pivot` từ đây để tạo `AlignmentResult`, không tự tính lại —
nếu hai nơi tính khác nhau thì toàn bộ vùng kiểm tra bị đặt lệch.

**`Components` được copy ra mảng riêng**, giống `InspectionSummary` — sửa list gốc sau khi tạo
recipe thì recipe vẫn giữ nguyên.

### Ràng buộc khi sửa code sau này: tên tham số constructor = tên property

Recipe sẽ được nạp từ file JSON. `System.Text.Json` dựng object qua **constructor**, và ghép
từng tham số với property **theo tên** (không phân biệt hoa thường):

```csharp
public Roi(double x, double y, double width, double height)   // x ↔ X, width ↔ Width ... ✔
```

Đặt tên tham số lệch đi (ví dụ `fid1` cho property `Fiducial1`) thì **vẫn compile được**,
nhưng đến lúc nạp file mới hỏng. Mỗi kiểu chỉ có **một** constructor public.

Đã kiểm thử:
- Ghi ra JSON rồi đọc lại → giống hệt ban đầu.
- File JSON viết tay kiểu `camelCase` (`"modelCode"`, `"minScore"`...) → đọc được, cần bật
  `PropertyNameCaseInsensitive = true` khi nạp.
- File JSON vi phạm ràng buộc (vùng xám vượt quá 1) → **văng lỗi ngay lúc nạp**, recipe sai
  không bao giờ lọt vào hệ thống.

> **Điểm cần xử lý sau (tầng Infrastructure):** khi **ghi** recipe ra JSON, các property tự tính
> (`Roi.Center`, `AlignmentSettings.Pivot`) cũng bị ghi ra. Đọc vào thì chúng bị bỏ qua nên không sai,
> nhưng người sửa tay file dễ bị lừa (đổi `X` mà `Center` vẫn giá trị cũ). Sẽ xử lý khi làm phần
> lưu recipe, để Core không phải biết gì về JSON.

### Nơi khác gọi đến như nào

**Hiện tại** — chưa có nơi nào gọi. Cách tạo một recipe:

```csharp
var recipe = new Recipe(
    modelCode: "PCB-A",
    version: 1,
    alignment: new AlignmentSettings(
        fiducial1: new Roi(20, 20, 20, 20),
        fiducial2: new Roi(580, 380, 20, 20),
        minScore: 0.8),
    components:
    [
        new ComponentSpec("R12", new Roi(100, 60, 40, 40)),
        new ComponentSpec("C5",  new Roi(200, 120, 30, 20))
    ],
    componentInspection: new ComponentInspectionSettings(
        presenceThreshold: 0.7, reviewMargin: 0.1, maxShiftPx: 8, maxRotationDegrees: 5));
```

**Sau này** *(minh hoạ, chưa có code)*:

- **Infrastructure** nạp recipe từ file JSON theo `ModelCode` mà MES trả về.
- **Bước định vị** đọc `recipe.Alignment`: tìm `Fiducial1`, `Fiducial2` trên ảnh chụp,
  so độ khớp với `MinScore` (thấp hơn → ERROR), tạo `AlignmentResult` với `Pivot = recipe.Alignment.Pivot`.
- **Bước kiểm linh kiện** duyệt `recipe.Components`, chuyển từng `Roi` sang ảnh chụp bằng
  `RoiTransformer`, chấm điểm, rồi phân vùng theo `recipe.ComponentInspection`.

### Kiểm chứng

Chưa có unit test. Đã kiểm bằng chương trình tạm ngoài repo — 21/21 đạt:
`Pivot` đúng tâm dấu định vị thứ nhất; copy phòng thủ; biên vùng xám đúng bằng 0 và 1 được chấp nhận;
vượt biên, biên âm, biên > 0.5 bị từ chối; các ràng buộc còn lại; ghi/đọc JSON; JSON camelCase;
JSON sai bị từ chối lúc nạp.

### Kiểm tra recipe hợp lệ — RecipeValidator (phần 3b)

#### Vai trò

Constructor chỉ kiểm được **từng object riêng lẻ** (chiều rộng > 0, vùng xám hợp lệ...).
Có những lỗi chỉ thấy khi nhìn **nhiều thành phần cùng lúc** — đó là việc của `RecipeValidator`.

#### Hai kiểu kiểm tra, hai cách báo lỗi

| | Kiểm trong constructor | `RecipeValidator` |
|---|---|---|
| Kiểm cái gì | ràng buộc của **một** object | ràng buộc **chéo** giữa nhiều thành phần |
| Sai thì sao | **văng exception** ở lỗi đầu tiên | trả về **danh sách tất cả lỗi** |
| Vì sao | object sai thì không được phép tồn tại | recipe do **người soạn tay**, cần thấy hết lỗi trong một lần để sửa một lượt |

Danh sách rỗng = recipe hợp lệ.

#### Xử lý như nào

| # | Luật | Thông báo ví dụ |
|---|---|---|
| 1 | Có ít nhất một linh kiện | `Recipe chưa có linh kiện nào để kiểm` |
| 2 | Không trùng mã linh kiện — **không phân biệt hoa thường** (`R12` và `r12` là trùng); mỗi mã trùng chỉ báo một lần | `Trùng mã linh kiện: R12` |
| 3 | Hai dấu định vị cách nhau ít nhất `RecipeLimits.MinFiducialDistancePx` (100 px) | `Hai dấu định vị cách nhau 50,0 px, cần tối thiểu 100 px để tính được góc xoay` |
| 4 | Mọi vùng kiểm tra (2 dấu định vị + mọi linh kiện) nằm trong khung ảnh; chạm đúng mép vẫn hợp lệ | `Vùng kiểm tra của R12 nằm ngoài khung ảnh 640×480` |

Lỗi trả về theo đúng thứ tự trên, linh kiện theo thứ tự trong recipe.

**Vì sao hai dấu định vị phải xa nhau?** Góc xoay được tính từ đoạn thẳng nối hai dấu.
Mỗi lần tìm dấu có thể sai lệch khoảng 1 px, nên sai số góc ≈ `1 px / khoảng cách`:

| Khoảng cách | Sai số góc khi lệch 1 px |
|---|---|
| 50 px | ≈ 1,15° |
| 100 px | ≈ 0,57° |
| 1000 px | ≈ 0,06° |

Hai dấu nằm sát nhau thì góc xoay đo ra gần như vô nghĩa. Thực tế nên đặt hai dấu ở **hai góc chéo nhau** của bo.

**Vì sao kích thước ảnh truyền vào làm tham số, không nằm trong recipe?** Recipe chỉ chứa cấu hình.
Kích thước ảnh là của **ảnh chuẩn** thật — phía Infrastructure biết được khi nạp ảnh đó lên.
Truyền kích thước ảnh ≤ 0 là **lỗi lập trình** chứ không phải lỗi người soạn recipe, nên vẫn văng exception.

Các con số và câu chữ đều nằm trong `Constants/` (`RecipeLimits`, `RecipeMessages`) — không viết thẳng trong code.
`double.Hypot(dx, dy)` là hàm có sẵn của .NET tính `√(dx² + dy²)`.

#### Nơi khác gọi đến như nào

**Hiện tại** — chưa có nơi nào gọi. Cách gọi:

```csharp
IReadOnlyList<string> errors = RecipeValidator.Validate(recipe, imageWidth: 640, imageHeight: 480);

if (errors.Count > 0)
{
    // recipe sai: không cho trạm chạy, hiện toàn bộ errors lên màn hình
}
```

**Sau này** *(minh hoạ)*: Infrastructure nạp file recipe JSON + ảnh chuẩn, gọi `Validate` với kích thước
ảnh chuẩn. Có lỗi thì **trạm không được khởi động**, HMI hiện danh sách lỗi cho kỹ sư sửa.

#### Kiểm chứng

Chưa có unit test. Đã kiểm bằng chương trình tạm ngoài repo — 13/13 đạt: recipe chuẩn không có lỗi;
không có linh kiện; trùng mã khác hoa thường chỉ báo một lần; dấu định vị quá gần; đúng bằng ngưỡng
thì chấp nhận; vùng kiểm tra lố mép phải, mép dưới, toạ độ âm; chạm đúng mép vẫn hợp lệ; dấu định vị
ra ngoài khung; **3 lỗi cùng lúc thì báo đủ cả 3**; kích thước ảnh = 0 văng exception.

### Chưa làm

Thứ tự và bật/tắt từng bước kiểm tra sẽ thêm ở phần 4, khi pipeline đã định nghĩa tên các bước.

---

## Đang chờ review

1. **`Failed` thắng `Error`?** Ví dụ: bước kiểm linh kiện thấy thiếu R12, bước kiểm bề mặt bị treo.
   - Đang chọn **NG** — bo đã chắc chắn thiếu linh kiện, kiểm lại vẫn ra NG.
   - Cách ngược lại: **ERROR** — có sự cố kỹ thuật là phải kiểm lại cho đủ, để truy vết không thiếu bước nào.
2. **`Passed` + `Skipped` = OK** có chấp nhận được không?

## Chạy test

```powershell
dotnet test tests\VisionApp.Core.Tests
```

## Nhật ký

| Phần | Nội dung |
|---|---|
| 1 | Mô hình kết luận: `InspectionDecision`, `StepStatus`, `StepResult`, `InspectionSummary`, `ResultAggregator` |
| 1b | Gom mọi câu chữ hiển thị về `InspectionMessages`, bỏ chuỗi viết thẳng trong code |
| 1c | Chia thư mục theo loại (`Constants/`, `Enums/`, `Models/`, `Services/`); thêm `DefectCodes` |
| 2 | Hình học: `Point2D`, `Roi`, `AlignmentResult`, `RotatedRoi`, `RoiTransformer` — chưa có unit test |
| 3a | Dữ liệu recipe: `Recipe`, `AlignmentSettings`, `ComponentSpec`, `ComponentInspectionSettings` — chưa có unit test |
| 3b | Kiểm tra recipe hợp lệ: `RecipeValidator`, `RecipeLimits`, `RecipeMessages` — chưa có unit test |
| 3c | Chuẩn hoá cấu trúc Clean Architecture: tách `Models/` thành `Entities/` + `ValueObjects/` (giữ tên project `VisionApp.Core`) |
| 3d | Đổi `record` → `class`: thêm `Common/ValueObject`; `Recipe` so sánh theo định danh; `StepResult.WithDuration` thay cho `with` |
| 3e | Chốt quy tắc **Entity = `class`, Value Object = `record`**: Value Object về lại `record`, bỏ `Common/ValueObject` và `WithDuration`; `InspectionSummary` dùng constructor + tự viết `Equals` cho danh sách; thêm kiểm `null` cho `StepResult.message` và phần tử trong `ResultAggregator` |
