# Dev 2 — Enemy & Wave System: Tiến độ & Lộ trình

## 1. Tiến độ hiện tại (Hoàn thành Giai đoạn 2: Core Gameplay)

Module Dev 2 đã hoàn thành nâng cấp hệ thống lên Giai đoạn 2.

### **Các tính năng đã hoàn thiện:**
- [x] **Hệ thống EnemyData ScriptableObject:** 
    - Quản lý thông số tập trung qua Asset.
    - Đã tạo sẵn 3 loại: Lính, Xe thiết giáp, Boss.
- [x] **Nâng cấp Wave & Spawning:**
    - `WaveManager` hỗ trợ nhiều loại enemy trong một wave.
    - `EnemySpawner` hỗ trợ sinh quân ngẫu nhiên trên nhiều Lane (hàng).
    - Cập nhật sự kiện `OnEnemyReachedBase` kèm theo sát thương cụ thể.
- [x] **Refactor mã nguồn:** Loại bỏ các lớp lỗi thời, code sạch và tối ưu hơn.

---

## 2. Lộ trình tiếp theo (Giai đoạn 2 & 4)

Dựa trên tài liệu `HON_VIET_PHASES.md`, Dev 2 sẽ tập trung vào các nhiệm vụ sau trong các giai đoạn tới:

### **Giai đoạn 2 — Core Gameplay (Ưu tiên cao)**
- [x] **Thiết lập thông số thật (Stats):**
    - Sử dụng `EnemyData` (ScriptableObject) để quản lý HP, Speed, Gold Reward, Damage to Base cho từng loại Enemy.
- [x] **Cấu hình 3 Wave chính thức:**
    - **Wave 1:** 5 Lính Xâm Lược.
    - **Wave 2:** 8 Lính Xâm Lược + 1 Xe Thiết Giáp.
    - **Wave 3:** 10 Lính Xâm Lược + 2 Xe Thiết Giáp + 1 Boss nhỏ.
- [x] **Logic phần thưởng:** Đảm bảo `goldReward` được gửi chính xác qua event `OnEnemyDied` để Dev 4 cộng tiền.

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
- **Cho Dev 4 (GameManager):** Hãy lắng nghe `OnEnemyDied(GameObject, int)` để lấy số tiền thưởng và `OnEnemyReachedBase(GameObject, float)` để trừ đúng lượng máu thành theo từng loại địch.
- **Cho Dev 5 (Art):** Lane hiện tại là đường thẳng (Straight lane), khi đưa vào Map Phù Đổng chỉ cần cập nhật lại vị trí các `LanePath` nodes.
