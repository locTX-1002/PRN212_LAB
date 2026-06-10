-- 1. Tạo Database
CREATE DATABASE ProductManagementDB;
GO

USE ProductManagementDB;
GO

-- 2. Tạo bảng AccountMembers
CREATE TABLE AccountMembers (
    MemberId NVARCHAR(50) PRIMARY KEY,
    MemberPassword NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100),
    EmailAddress NVARCHAR(100),
    MemberRole INT
);
GO

-- 3. Tạo bảng Categories
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY, -- IDENTITY(1,1) để tự động tăng ID
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- 4. Tạo bảng Products
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY, -- Tự động tăng ID
    ProductName NVARCHAR(100) NOT NULL,
    CategoryId INT,
    UnitsInStock SMALLINT,
    UnitPrice DECIMAL(18, 2),
    -- Ràng buộc khóa ngoại nối sang bảng Categories
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- 5. Insert Dữ liệu mẫu (Seed Data) để test đăng nhập và hiển thị
-- Tài khoản Admin (Mật khẩu: @1)
INSERT INTO AccountMembers (MemberId, MemberPassword, FullName, EmailAddress, MemberRole)
VALUES ('PS0001', '@1', 'Administrator', 'admin@fpt.edu.vn', 1);

-- Dữ liệu danh mục (Cho cboCategory)
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (CategoryId, CategoryName) VALUES 
(1, 'Beverages'),
(2, 'Condiments'),
(3, 'Confections'),
(4, 'Dairy Products'),
(5, 'Seafood');
SET IDENTITY_INSERT Categories OFF;

-- Dữ liệu Sản phẩm mẫu (Cho DataGrid)
INSERT INTO Products (ProductName, CategoryId, UnitsInStock, UnitPrice) VALUES 
('Chai', 1, 39, 18.00),
('Chang', 1, 17, 19.00),
('Aniseed Syrup', 2, 13, 10.00),
('Chef Anton Cajun Seasoning', 2, 53, 22.00),
('Ikura', 5, 31, 31.00);
GO

PRINT 'Tạo Database thành công!';