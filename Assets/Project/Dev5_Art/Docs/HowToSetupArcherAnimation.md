# Hướng dẫn thiết lập Animation cho Archer (Archer Animation Setup Guide)

Tài liệu này hướng dẫn bạn cách kéo model Archer thật và gán các animation clip vào hệ thống Animator Controller đã được tạo sẵn trong Phase 19A.

---

## Quy trình gán Model và Animation

### Bước 1: Mở Prefab Archer
- Tìm prefab Archer tại: `Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab`.
- Nhấp đúp (Double-click) hoặc chọn **Open Prefab** để vào chế độ chỉnh sửa Prefab Mode.

### Bước 2: Kéo Model vào `ModelSlot`
- Tìm đến child GameObject: `Visual` -> `ModelSlot`.
- Kéo model Archer (file FBX hoặc model prefab) của bạn thả vào làm **con trực tiếp** của `ModelSlot`.

### Bước 3: Cấu hình Animator trên Model con
- Chọn model Archer vừa kéo vào `ModelSlot`.
- Đảm bảo model con có gắn component **Animator**. Nếu chưa có, hãy nhấn **Add Component** -> **Animator**.
- Gán controller đã được chuẩn bị sẵn vào trường **Controller** của Animator:
  `Assets/Project/Dev5_Art/Animations/Archer/Archer_AnimatorController.controller`
- **LƯU Ý QUAN TRỌNG:**
  - Hãy bỏ chọn (Tắt/Uncheck) thuộc tính **Apply Root Motion** trên component Animator này.
  - Phải đặt `Apply Root Motion = false` để tránh việc animation dịch chuyển vị trí thực tế của unit trên bàn cờ. Mọi di chuyển vật lý vẫn do script gameplay điều khiển.

### Bước 4: Chỉnh sửa kích thước và trục xoay của Model
Nếu model xuất hiện lệch vị trí, kích cỡ hoặc góc quay:
- **Nếu model quá to hoặc quá nhỏ:** Chỉnh thông số **Scale** của model con. Thử các giá trị:
  - `0.01` (thông dụng khi xuất từ Blender)
  - `0.02` / `0.05` / `0.1` / `1`
- **Nếu model bị chìm hoặc bay lơ lửng:** Chỉnh **Position Y** của model con cho khớp mặt đất.
- **Nếu model quay sai hướng:** Chỉnh **Rotation Y** của model con thành:
  - `90` / `180` / `-90` (thông thường `180` để mặt quay về phía trước).

### Bước 5: Kéo thả các Animation Clip thật
- Mở Animator Window bằng cách vào thanh menu: **Window** -> **Animation** -> **Animator**.
- Chọn file: `Assets/Project/Dev5_Art/Animations/Archer/Archer_AnimatorController.controller`.
- Bạn sẽ thấy 4 trạng thái (States) đã được kết nối transitions:
  1. **Idle** (Đang chạy `Archer_Idle_Placeholder.anim`)
  2. **Move** (Đang chạy `Archer_Move_Placeholder.anim`)
  3. **Attack** (Đang chạy `Archer_Attack_Placeholder.anim`)
  4. **Death** (Đang chạy `Archer_Death_Placeholder.anim`)
- Chọn từng State, sau đó kéo các animation clip thật của Archer (như Idle, Run, Walk, Attack, Die) thả vào trường **Motion** trong cửa sổ Inspector của State tương ứng để ghi đè các clip placeholder.

---

## Cơ chế hoạt động của Script & Animator

- **CharacterAnimationController.cs** gắn trên root prefab sẽ tự động tìm Animator của model thật trong `ModelSlot`.
- **UnitAutoAttack.cs** tự động kích hoạt:
  - `SetMoving(true)` khi Archer đang di chuyển tới mục tiêu trong phạm vi đánh → Kích hoạt Parameter `IsMoving` = true → Chuyển sang State `Move`.
  - `SetMoving(false)` khi Archer đứng yên chờ hoặc trong phạm vi tấn công → Kích hoạt Parameter `IsMoving` = false → Chuyển sang State `Idle`.
  - `PlayAttack()` khi Archer thực hiện bắn tên → Kích hoạt Trigger `Attack` → Chuyển sang State `Attack` (sau đó tự động trả về `Idle` khi chạy xong).
  - Khi chết, sự kiện `Health.OnDeath` sẽ kích hoạt Trigger `Death` → Chuyển sang State `Death`.

---

## Kiểm tra và Test trong Play Mode

1. Thoát khỏi Prefab Mode.
2. Bấm **Play** trong Unity Editor.
3. Mua Archer trong Shop và đặt lên Board.
4. Nhấn **Start Battle**:
   - Khi Archer di chuyển lại gần kẻ địch, hãy đảm bảo trạng thái chuyển đổi mượt mà từ **Idle** sang **Move**.
   - Khi Archer dừng lại bắn tên, chuyển về **Idle** và chạy animation **Attack**.
   - Khi Archer bắn, projectile (mũi tên) vẫn bay ra đúng hướng và chỉ gây sát thương khi va chạm vào kẻ địch.
   - Khi Archer chết, animation **Death** được kích hoạt trước khi biến mất.
