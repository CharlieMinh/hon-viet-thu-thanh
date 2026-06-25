# Hướng dẫn thay thế Model Nhân vật (Character Model Replacement Guide)

Tài liệu này hướng dẫn bạn cách thay thế các hình khối placeholder (như Capsule/Cube) bằng model 3D thật (ví dụ: FBX, OBJ hoặc Prefab Model) cho các đơn vị Unit (Hero) và Enemy mà không làm ảnh hưởng đến logic gameplay hay hệ thống Collider hiện có.

---

## Các bước thay thế Model Nhân vật

### Bước 1: Mở Prefab cần thay thế
- Trong cửa sổ **Project**, tìm đến prefab của nhân vật hoặc quái vật cần chỉnh sửa:
  - **Unit (Hero):** `Assets/Project/Dev5_Art/Prefabs/Heroes/` (ví dụ: `Knight_Unit_Prefab.prefab`)
  - **Enemy (Quái):** `Assets/Project/Dev5_Art/Prefabs/Enemies/` (ví dụ: `Goblin_Enemy_Prefab.prefab`)
- Nhấp đúp (Double-click) vào prefab hoặc nhấn **Open Prefab** để vào chế độ chỉnh sửa Prefab Mode.

### Bước 2: Tìm và Kéo Model thật vào `ModelSlot`
- Trong cây phân cấp (Hierarchy) của Prefab, tìm child theo đường dẫn:
  `Visual` -> `ModelSlot`
- Kéo model thật (file FBX hoặc model prefab) từ cửa sổ Project thả vào làm **con trực tiếp** của `ModelSlot`.

### Bước 3: Điều chỉnh Transform của Model
Thông thường, model nhập từ Blender/Maya/Max có thể bị sai tỷ lệ (Scale), bị lệch trục xoay (Rotation) hoặc bị lún/chìm xuống đất (Position Y). Hãy chỉnh sửa các thông số Transform của **model con** vừa kéo vào (không chỉnh sửa chính `ModelSlot` để giữ nó ở vị trí `(0,0,0)` mặc định).

#### Hướng dẫn Scale Model:
- Nếu model quá to hoặc quá nhỏ, hãy điều chỉnh thông số Scale của model con. Các tỉ lệ scale phổ biến để thử:
  - **`0.01`** (Thường dùng cho model xuất từ Blender không đặt đúng đơn vị hệ mét)
  - **`0.02`** / **`0.05`** / **`0.1`**
  - **`1`** (Nếu model đã được chuẩn hóa hệ mét)

#### Hướng dẫn Trục Xoay (Rotation Y):
- Nếu nhân vật quay mặt sai hướng (ví dụ: quay lưng lại hướng đi hoặc quay ngang):
  - Hãy chỉnh góc xoay **Rotation Y** của model con thành: **`90`**, **`180`**, hoặc **`-90`** (thông thường là `180` để hướng về phía trước đúng chuẩn camera).

#### Hướng dẫn Độ cao (Position Y):
- Nếu chân nhân vật bị chìm dưới mặt đất (lún) hoặc bay lơ lửng:
  - Hãy điều chỉnh trục **Position Y** của model con tăng hoặc giảm nhẹ cho đến khi chân nhân vật chạm đúng vạch mặt đất (đường lưới Y = 0).

### Bước 4: Ẩn Visual Placeholder
- Script `CharacterVisualSlot` sẽ tự động ẩn `Visual/Placeholder` khi bạn chạy game hoặc trong Editor nhờ cơ chế tự động kiểm tra xem `ModelSlot` có chứa model con hay không.
- Nếu bạn muốn ẩn hoàn toàn trong lúc làm việc trong Prefab Mode, bạn có thể tắt (deactivate) GameObject `Visual/Placeholder` bằng cách bỏ tích ở ô Checkbox trên cùng của Inspector.

### Bước 5: Giữ nguyên Collider và Script ở Root
- **LƯU Ý QUAN TRỌNG:**
  - Không được di chuyển, thay thế hay xóa các script gameplay đang gắn trên **Root** của Prefab (ví dụ: `PlaceableUnit`, `Health`, `UnitCombatStats`, `EnemyController`, v.v.).
  - Không được xóa hay vô hiệu hóa **Collider** ở Root (ví dụ: `CapsuleCollider` hoặc `BoxCollider`). Collider này dùng để tính toán tầm đánh, trúng đạn và click chuột chọn unit.
  - Không sử dụng `MeshCollider` của model thật làm collider gameplay vì nó quá nặng và làm sai lệch combat logic. Model thật chỉ thuần túy có vai trò hiển thị trực quan (visual).

### Bước 6: Kiểm tra trong Play Mode
- Thoát khỏi Prefab Mode.
- Nhấn **Play** và chạy thử game.
- Mua Unit trong Shop hoặc chờ Wave enemy spawn:
  - Kiểm tra xem model thật có hiển thị đúng vị trí không.
  - Kiểm tra xem thanh máu (`HealthBar`) và cấp sao (`StarText`) có hiển thị chuẩn xác ở trên đầu nhân vật không.
  - Kiểm tra xem khi click chuột phải có mở được **Inspect Panel** xem thông tin không.
  - Đảm bảo các đơn vị di chuyển, chiến đấu, nhận sát thương và hồi sinh/bị huỷ bình thường mà không gây ra bất cứ thông báo lỗi `NullReferenceException` hay `MissingReferenceException` nào trong Unity Console.
