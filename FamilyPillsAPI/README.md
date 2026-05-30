# FamilyPills Backend API - Hướng dẫn cài đặt và sử dụng

## 1. Yêu cầu hệ thống

- .NET 8.0 SDK trở lên
- MySQL 8.0 trở lên
- Visual Studio 2022 hoặc Visual Studio Code

## 2. Các bước cài đặt

### 2.1 Cài đặt các NuGet Package

Mở Package Manager Console trong Visual Studio và chạy lệnh:

```bash
dotnet restore
```

Hoặc nếu bạn chưa cài đặt các package, hãy chạy:

```bash
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
```

### 2.2 Tạo Database

1. Mở MySQL Workbench hoặc MySQL Command Line
2. Chạy script `create-database.sql` để tạo database và bảng

```bash
mysql -u root -p < create-database.sql
```

### 2.3 Cập nhật Connection String

Mở file `appsettings.json` và cập nhật connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=FamilyPillsDB;User=root;Password=YOUR_PASSWORD;"
}
```

### 2.4 Chạy ứng dụng

```bash
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

## 3. API Endpoints

### 3.1 Lấy danh sách tất cả thuốc

**Request:**
```
GET /api/medicines
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Aspirin",
    "barcode": "1234567890",
    "totalQuantity": 100,
    "unit": "viên",
    "expiryDate": "2025-12-31",
    "imagePath": "",
    "quantity": "100",
    "lastUpdated": "2025-05-30 10:00:00",
    "isRunningLow": false,
    "isExpired": false
  }
]
```

### 3.2 Lấy chi tiết một thuốc

**Request:**
```
GET /api/medicines/{id}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Aspirin",
  "barcode": "1234567890",
  "totalQuantity": 100,
  "unit": "viên",
  "expiryDate": "2025-12-31",
  "imagePath": "",
  "quantity": "100",
  "lastUpdated": "2025-05-30 10:00:00",
  "isRunningLow": false,
  "isExpired": false
}
```

**Response (404 Not Found):**
```json
{
  "message": "Không tìm thấy thuốc với Id 999"
}
```

### 3.3 Thêm thuốc mới

**Request:**
```
POST /api/medicines
Content-Type: application/json

{
  "name": "Ibuprofen",
  "barcode": "1234567893",
  "totalQuantity": 50,
  "unit": "viên",
  "expiryDate": "2025-11-30",
  "imagePath": "/images/ibuprofen.png",
  "quantity": "50",
  "isRunningLow": false,
  "isExpired": false
}
```

**Response (201 Created):**
```json
{
  "id": 4,
  "name": "Ibuprofen",
  "barcode": "1234567893",
  "totalQuantity": 50,
  "unit": "viên",
  "expiryDate": "2025-11-30",
  "imagePath": "/images/ibuprofen.png",
  "quantity": "50",
  "lastUpdated": "2025-05-30 10:05:00",
  "isRunningLow": false,
  "isExpired": false
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Tên thuốc không được để trống"
}
```

### 3.4 Cập nhật thuốc

**Request:**
```
PUT /api/medicines/{id}
Content-Type: application/json

{
  "name": "Ibuprofen 200mg",
  "barcode": "1234567893",
  "totalQuantity": 30,
  "unit": "viên",
  "expiryDate": "2025-11-30",
  "imagePath": "/images/ibuprofen.png",
  "quantity": "30",
  "isRunningLow": true,
  "isExpired": false
}
```

**Response (200 OK):**
```json
{
  "message": "Cập nhật thuốc thành công",
  "data": {
    "id": 4,
    "name": "Ibuprofen 200mg",
    "barcode": "1234567893",
    "totalQuantity": 30,
    "unit": "viên",
    "expiryDate": "2025-11-30",
    "imagePath": "/images/ibuprofen.png",
    "quantity": "30",
    "lastUpdated": "2025-05-30 10:10:00",
    "isRunningLow": true,
    "isExpired": false
  }
}
```

### 3.5 Xóa thuốc

**Request:**
```
DELETE /api/medicines/{id}
```

**Response (200 OK):**
```json
{
  "message": "Xóa thuốc thành công",
  "deletedMedicineId": 4
}
```

**Response (404 Not Found):**
```json
{
  "message": "Không tìm thấy thuốc với Id 999"
}
```

## 4. Swagger UI

Khi chạy ứng dụng ở môi trường Development, bạn có thể truy cập Swagger UI tại:

```
https://localhost:5001/swagger/index.html
```

hoặc

```
http://localhost:5000/swagger/index.html
```

Swagger UI cho phép bạn kiểm tra, test tất cả các API endpoints một cách dễ dàng.

## 5. Cấu trúc dự án

```
FamilyPillsAPI/
├── Controllers/
│   └── MedicinesController.cs       # API endpoints
├── Models/
│   └── Medicine.cs                  # Model dữ liệu
├── Data/
│   └── FamilyPillsDbContext.cs      # DbContext (Entity Framework)
├── Program.cs                       # Cấu hình ứng dụng
├── appsettings.json                 # Cài đặt (connection string, ...)
├── appsettings.Development.json     # Cài đặt cho môi trường Development
├── FamilyPillsAPI.csproj           # Project file
├── create-database.sql              # Script tạo database
└── README.md                        # Tài liệu này
```

## 6. Lưu ý

- Tất cả các response lỗi đều có message tiếng Việt
- Thời gian (LastUpdated) được lưu dưới định dạng `yyyy-MM-dd HH:mm:ss`
- Các field không bắt buộc có thể để trống (string rỗng hoặc null)
- Số lượng thuốc (TotalQuantity) phải >= 0

## 7. Hỗ trợ

Nếu có vấn đề, hãy:
1. Kiểm tra connection string trong `appsettings.json`
2. Đảm bảo MySQL server đang chạy
3. Kiểm tra logs trong Visual Studio Output window
4. Tham khảo Entity Framework Core documentation: https://docs.microsoft.com/en-us/ef/core/
