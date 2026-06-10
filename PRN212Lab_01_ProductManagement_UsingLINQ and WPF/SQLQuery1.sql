CREATE DATABASE MyStoreDB;
GO
USE MyStoreDB;
GO

CREATE TABLE AccountMember (
    MemberId VARCHAR(20) PRIMARY KEY,
    MemberPassword VARCHAR(50),
    FullName NVARCHAR(100),
    EmailAddress VARCHAR(100),
    MemberRole INT
);

-- Tạo tài khoản để test đăng nhập
INSERT INTO AccountMember VALUES ('PS0001', '@1', 'Admin User', 'admin@fpt.edu.vn', 1);
EXEC sp_rename 'AccountMember', 'AccountMembers';
