# Playtest Checklist - Phase 17: Game Config & Clean Prototype Setup

Tài liệu này hướng dẫn các bước kiểm thử chi tiết để xác nhận hệ thống cấu hình game (`GameConfig`), cấu trúc Prefab mới và các tính năng cốt lõi hoạt động ổn định.

---

## 1. Test 1: Chế độ thường (Normal Mode)
Mục tiêu: Đảm bảo game chạy bình thường, cân bằng cho người chơi, không chứa các yếu tố thử nghiệm (debug).

* [ ] Chọn GameObject `GameConfig` trong Scene `Scene_Dev5_Art`:
  * Thiết lập `debugMode = false`.
  * Thiết lập `startingGoldNormal = 10` (hoặc `12`).
  * Thiết lập `enableDebugHotkeys = true` hoặc `false`.
* [ ] Nhấn **Play** trong Unity Editor.
* [ ] **Xác nhận starting gold**: Số vàng hiển thị ban đầu phải đúng bằng `startingGoldNormal` (ví dụ: 10 Gold).
* [ ] **Xác nhận cờ test**: Không có bất kỳ cờ test nào tự động sinh ra trên bàn cờ hoặc hàng chờ (Không có `Knight_TestUnit`, `Archer_TestUnit`, `Tank_TestUnit`).
* [ ] **Xác nhận debug hotkey**: Nhấn phím `T` trên bàn phím. Xác nhận quái không bị mất máu và không có lỗi nào xuất hiện trong console (phím `T` bị khóa ở Normal Mode).

---

## 2. Test 2: Chế độ Debug (Debug Mode)
Mục tiêu: Đảm bảo chế độ Debug giúp lập trình viên thử nghiệm game nhanh chóng.

* [ ] Chọn GameObject `GameConfig` trong Scene:
  * Thiết lập `debugMode = true`.
  * Thiết lập `startingGoldDebug = 1000`.
  * Thiết lập `enableDebugHotkeys = true`.
* [ ] Nhấn **Play** trong Unity Editor.
* [ ] **Xác nhận starting gold**: Số vàng hiển thị ban đầu phải là 1000 Gold.
* [ ] **Xác nhận cờ test**: Có 3 cờ test (`Knight_TestUnit`, `Archer_TestUnit`, `Tank_TestUnit`) tự động xuất hiện ở khu vực chờ/bàn cờ để test nhanh.
* [ ] **Xác nhận debug hotkey**: Nhấn phím `T` trên bàn phím. Xác nhận quái đầu tiên trong danh sách bị trừ 10 máu (phím `T` hoạt động bình thường).

---

## 3. Test 3: Cấu trúc Prefab & Art Visual
Mục tiêu: Xác nhận cấu trúc prefab sạch giúp ghép art dễ dàng, không phá hỏng logic.

* [ ] Mở prefab `Knight_Unit_Prefab` (hoặc `Archer_Unit_Prefab`, `Tank_Unit_Prefab`).
* [ ] **Xác nhận cấu trúc**:
  * Root GameObject chỉ chứa logic scripts (`PlaceableUnit`, `UnitCombatStats`, `UnitAutoAttack`, `Health`, v.v.) và `CapsuleCollider`.
  * Có child `Visual` chứa `MeshFilter` và `MeshRenderer`.
  * Có child `UI` chứa `HealthBar` và `StarText`.
* [ ] Vào Play Mode, chuột phải vào cờ để hiển thị `InspectPanel`.
* [ ] **Test Visual Swap**: Thử tắt/ẩn child `Visual` hoặc thay thế Mesh/Material trong child `Visual`.
  * Xác nhận: Cờ vẫn có thể kéo thả, di chuyển, chiến đấu và cập nhật thanh máu/sao bình thường.

---

## 4. Test 4: Luồng chơi chính (Core Gameplay Loop)
Mục tiêu: Đảm bảo các thay đổi không làm hỏng gameplay hiện có.

* [ ] **Mua quân (Shop)**: Click mua Knight/Archer/Tank trong ShopCanvas. Xác nhận trừ tiền đúng và cờ xuất hiện trên hàng chờ.
* [ ] **Đặt quân (Placement)**: Click chuột trái chọn cờ trên hàng chờ và click chuột trái đặt lên ô bàn cờ.
* [ ] **Nâng sao (Upgrade)**: Mua 3 cờ cùng loại. Xác nhận tự động nâng sao và chỉ số tăng lên (kiểm tra qua Inspect Panel).
* [ ] **Tương tác chuột (Input)**:
  * Chuột trái: Chọn và đặt quân.
  * Chuột phải vào quân cờ / kẻ địch: Hiển thị bảng thông tin chi tiết (`InspectPanel`).
  * Chuột phải nền trống / nhấn `ESC`: Bỏ chọn quân đang cầm và ẩn `InspectPanel`.
  * Xác nhận không bị xung đột thao tác chuột.
* [ ] **Bắt đầu trận đấu (Combat)**: Click **Start Battle**.
  * Xác nhận trạng thái game chuyển sang `Combat`.
  * Xác nhận quái xuất hiện từ điểm sinh và di chuyển tấn công.
  * Xác nhận cờ người chơi tự động tìm mục tiêu và tấn công.
* [ ] **Lợi tức (Interest)**: Đợi wave completed. Xác nhận cộng 5 Gold lợi tức cho mỗi 10 Gold người chơi đang sở hữu (nếu có).
* [ ] **Thắng/Thua (Victory/Defeat)**:
  * Nếu diệt hết quái: Trạng thái chuyển sang `WaveCompleted` (hoặc `Win` nếu ở wave cuối). Quân cờ còn sống tự reset về ô cũ.
  * Nếu quân cờ bị diệt hết: Trạng thái chuyển sang `Lose`.
