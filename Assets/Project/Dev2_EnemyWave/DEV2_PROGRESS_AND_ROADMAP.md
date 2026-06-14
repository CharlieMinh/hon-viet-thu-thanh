# Dev 2 — Enemy & Wave System: Tiến độ & Lộ trình

## 1. Tiến độ hiện tại (Hoàn thành Giai đoạn 1: Prototype)

Hiện tại, Module Dev 2 đã hoàn thành các tính năng cốt lõi cho bản Prototype thô. Toàn bộ mã nguồn và asset đã được commit lên nhánh `feature/dev2`.

### **Các tính năng đã hoàn thiện:**
- [x] **Hệ thống Enemy cơ bản:**
    - Tạo Prefab `Enemy_Prototype` (Cube placeholder).
    - Implement `IDamageable` và `ITargetable` (Sẵn sàng cho Dev 3 - Combat).
    - Tự động hủy/trả về Pool khi chết hoặc chạm đích.
- [x] **Hệ thống Di chuyển (Movement):**
    - `LanePath`: Xác định điểm bắt đầu và kết thúc của lane.
    - `EnemyMover`: Di chuyển enemy mượt mà giữa các điểm.
- [x] **Quản lý Wave & Spawning:**
    - `EnemySpawner`: Sinh enemy từ Pool tại điểm bắt đầu lane.
    - `WaveManager`: Quản lý danh sách các đợt tấn công (Wave).
    - `EnemyPool`: Tối ưu hiệu suất bằng Object Pooling.
- [x] **Hệ thống Sự kiện (Events):**
    - Kết nối thành công với `GameEvents`: `OnEnemySpawned`, `OnEnemyDied`, `OnEnemyReachedBase`, `OnWaveStarted`, `OnWaveCompleted`.
- [x] **Công cụ Debug:**
    - `EnemyWaveDebugInput`: Cho phép test nhanh (Space: Spawn, K: Kill, R: Reset, N: Next Wave).

---

## 2. Lộ trình tiếp theo (Giai đoạn 2 & 4)

Dựa trên tài liệu `HON_VIET_PHASES.md`, Dev 2 sẽ tập trung vào các nhiệm vụ sau trong các giai đoạn tới:

### **Giai đoạn 2 — Core Gameplay (Ưu tiên cao)**
- [ ] **Thiết lập thông số thật (Stats):**
    - Sử dụng `EnemyData` (ScriptableObject) để quản lý HP, Speed, Gold Reward, Damage to Base cho từng loại Enemy.
- [ ] **Cấu hình 3 Wave chính thức:**
    - **Wave 1:** 5 Lính Xâm Lược.
    - **Wave 2:** 8 Lính Xâm Lược + 1 Xe Thiết Giáp.
    - **Wave 3:** 10 Lính Xâm Lược + 2 Xe Thiết Giáp + 1 Boss nhỏ.
- [ ] **Logic phần thưởng:** Đảm bảo `goldReward` được gửi chính xác qua event `OnEnemyDied` để Dev 4 cộng tiền.

### **Giai đoạn 4 — Content (Thay thế Asset)**
- [ ] **Tích hợp Model 3D Low-Poly:**
    - Thay thế Cube bằng model Lính Xâm Lược, Xe Thiết Giáp và Boss.
- [ ] **Cập nhật Animation:** Kết nối logic di chuyển với Walk animation từ Dev 5.

### **Giai đoạn 5 — Polish**
- [ ] **Hiệu ứng & Âm thanh:**
    - Thêm hiệu ứng hạt (Particle) khi enemy tan biến.
    - Thêm âm thanh khi enemy chết hoặc khi bắt đầu wave mới.

---

## 3. Ghi chú bàn giao (Handoff)
- **Cho Dev 3 (Combat):** Enemy đã có Component `Enemy` implement `IDamageable`. Hãy dùng `TakeDamage(float)` để trừ máu enemy.
- **Cho Dev 4 (GameManager):** Hãy lắng nghe `OnEnemyDied(GameObject, int)` để lấy số tiền thưởng và `OnEnemyReachedBase(GameObject)` để trừ máu thành.
- **Cho Dev 5 (Art):** Lane hiện tại là đường thẳng (Straight lane), khi đưa vào Map Phù Đổng chỉ cần cập nhật lại vị trí các `LanePath` nodes.
