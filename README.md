# 💒 Wedding House - Hệ Thống Quản Lý Tiệc Cưới Trực Tuyến

![.NET Core](https://img.shields.io/badge/.NET%20Core-ASP.NET-purple?style=flat-square)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red?style=flat-square)
![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap%205-blue?style=flat-square)

## 📖 Giới thiệu tổng quan
**Wedding House** là một ứng dụng Web toàn diện được thiết kế để số hóa quy trình đặt tiệc cưới. Hệ thống cung cấp trải nghiệm đặt tiệc trực tuyến mượt mà cho khách hàng (B2C) và cung cấp bộ công cụ quản lý nghiệp vụ, doanh thu tối ưu cho phía nhà hàng (B2B). 

Dự án chú trọng vào tính chính xác của dữ liệu, bảo mật người dùng và tối ưu hóa quy trình nghiệp vụ thực tế.

---

## ✨ Tính năng nổi bật

###  Phân hệ Khách hàng (Customer)
* **Xác thực an toàn:** Đăng ký và khôi phục mật khẩu bảo mật qua hệ thống mã OTP gửi tự động vào Gmail cá nhân.
* **Đặt tiệc thông minh (4 bước):**
  1. Chọn Không gian & Thời gian (Sảnh, Ca, Số bàn).
  2. Chọn Thực đơn (từ khai vị đến tráng miệng).
  3. Chọn Dịch vụ đi kèm (MC, Âm thanh, Trang trí,...).
  4. Xác nhận báo giá tự động.
* **Quản lý đơn hàng:** Tra cứu lịch sử đặt tiệc và theo dõi trạng thái hợp đồng (Chờ duyệt, Đã cọc, Đã tất toán).

###  Phân hệ Quản trị (Admin)
* **Cấu hình động:** Tùy chỉnh linh hoạt các quy định (số bàn tối đa, giá bàn tối thiểu) cho từng phân hạng sảnh (A, B, C...).
* **Nghiệp vụ Tất toán:** Xử lý hợp đồng, tính toán tiền còn lại và xuất hóa đơn sau khi tiệc kết thúc.
* **Báo cáo Thống kê:** Dashboard trực quan tự động tính toán tổng doanh thu theo tháng thực tế.

---

##  Điểm nhấn Kỹ thuật (Technical Highlights)
1. **Real-time Validation (Kiểm duyệt thời gian thực):** Hệ thống tự động chặn các lỗi vượt quá sức chứa sảnh hoặc trùng lịch ngay tại thời điểm khách hàng thao tác, đảm bảo không xảy ra xung đột dữ liệu.
2. **Smart Cleanup System (Dọn rác thông minh):** Tự động phát hiện và xóa các tài khoản rác (người dùng thoát trang khi chưa xác thực OTP) để tối ưu hóa không gian Database và giải phóng Username.
3. **Security First (Bảo mật tối đa):** Áp dụng **JWT (JSON Web Token)** để phân quyền quản trị và mã hóa mật khẩu một chiều bằng thuật toán **BCrypt** chống lộ lọt dữ liệu.

---

##  Công nghệ sử dụng

| Tầng (Layer) | Công nghệ |
| :--- | :--- |
| **Backend** | ASP.NET Core (C#), RESTful API |
| **Database** | Microsoft SQL Server, Entity Framework Core |
| **Frontend** | HTML5, CSS3, Bootstrap 5.3, Vanilla JS (Fetch API) |
| **Libraries/Tools**| BCrypt.Net, JWT, MailKit / MimeKit (SMTP) |

---

##  Hướng dẫn Cài đặt & Chạy dự án (Localhost)

```bash
**1. Tải mã nguồn**
git clone [https://github.com/Aduanhduy09/Wedding_House.git](https://github.com/Aduanhduy09/Wedding_House.git)

2. Thiết lập Cơ sở dữ liệu (Database)**
* Mở hệ quản trị CSDL SQL Server Management Studio (SSMS).
* Tạo một Database trống mới với tên là `WeddingHouse`.
* Mở file script `Database/WeddingHouse_DB.sql` đã được đính kèm sẵn trong mã nguồn dự án.
* Nhấn **Execute (F5)** để hệ thống tự động khởi tạo toàn bộ cấu trúc bảng, các mối quan hệ và thêm dữ liệu mẫu ban đầu.

3. Cấu hình ứng dụng

Mở Solution (.sln) bằng Visual Studio 2022.

Cập nhật chuỗi kết nối (ConnectionStrings) trong file appsettings.json cho khớp với SQL Server của máy tính.

Cập nhật App Password của Gmail trong Controllers/AuthController.cs (nếu cần test tính năng gửi Email OTP).

4. Khởi chạy

Chuột phải vào Solution -> Chọn Restore NuGet Packages.

Nhấn Ctrl + Shift + B để biên dịch (Build) dự án.

Nhấn F5 hoặc nút Play để khởi chạy ứng dụng trên trình duyệt web.
```
## 💖 Lời cảm ơn (Acknowledgments)

Dự án **Wedding House** được hoàn thiện không chỉ nhờ sự nỗ lực của bản thân mà còn nhờ sự hướng dẫn và hỗ trợ nhiệt tình từ nhiều phía. Em xin gửi lời cảm ơn chân thành đến:

* **Thầy Huỳnh Ngọc Tín:** Đã tận tình hướng dẫn, định hướng kiến trúc hệ thống và góp ý để dự án bám sát thực tế nhất.
* **Trường Đại học Công nghệ Thông tin (UIT):** Đã tạo môi trường học tập và cung cấp nền tảng kiến thức vững chắc để mình có thể xây dựng và phát triển phần mềm này.
* Các nguồn tài liệu, cộng đồng mã nguồn mở (StackOverflow, GitHub) và các tác giả của những thư viện (MailKit, BCrypt.Net, Bootstrap) đã giúp dự án chạy mượt mà.
