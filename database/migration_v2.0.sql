-- ============================================
-- 声域友 KTV System - Migration v2.0
-- 产品方向转型：线下KTV管理 → 线上社交KTV平台
-- ============================================

BEGIN TRANSACTION;

-- 1. Users 表添加 Email 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Email')
BEGIN
    ALTER TABLE Users ADD Email NVARCHAR(100) NULL;
    -- 添加唯一约束
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;
END

-- 2. 重建 Rooms 表（从物理包厢改为虚拟房间）
-- 先清理依赖 PlayQueue 的外键
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PlayQueue_Rooms')
BEGIN
    ALTER TABLE PlayQueue DROP CONSTRAINT FK_PlayQueue_Rooms;
END

-- 备份旧 Rooms 数据到临时表
IF OBJECT_ID('tempdb..#OldRooms') IS NOT NULL DROP TABLE #OldRooms;
SELECT * INTO #OldRooms FROM Rooms;

-- 删除旧 Rooms 表
DROP TABLE Rooms;

-- 创建新的虚拟 Rooms 表
CREATE TABLE Rooms (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoomCode NVARCHAR(10) NOT NULL UNIQUE,
    Status NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedByUserId INT NOT NULL,
    CurrentUsers INT NOT NULL DEFAULT 0,
    IdleCloseAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ClosedAt DATETIME2 NULL,
    CONSTRAINT FK_Rooms_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- 重新建立 PlayQueue 到 Rooms 的外键
ALTER TABLE PlayQueue ADD CONSTRAINT FK_PlayQueue_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id);

-- 3. 创建 RoomRequests 表（开房申请）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomRequests')
BEGIN
    CREATE TABLE RoomRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
        RoomId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL,
        ProcessedBy INT NULL,
        CONSTRAINT FK_RoomRequests_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_RoomRequests_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
    );
END

-- 4. 创建 Feedbacks 表（用户反馈）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Feedbacks')
BEGIN
    CREATE TABLE Feedbacks (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FeedbackType NVARCHAR(30) NOT NULL,
        SongName NVARCHAR(200) NULL,
        Artist NVARCHAR(200) NULL,
        Description NVARCHAR(1000) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL,
        CONSTRAINT FK_Feedbacks_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END

-- 5. 删除 Orders 表（移除订单系统）
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    -- 先清理外键约束
    DECLARE @fkName NVARCHAR(200);
    SELECT @fkName = name FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Orders');
    IF @fkName IS NOT NULL
        EXEC('ALTER TABLE Orders DROP CONSTRAINT ' + @fkName);
    DROP TABLE Orders;
END

-- 6. 删除 Holidays 表（移除节假日定价）
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Holidays')
BEGIN
    DROP TABLE Holidays;
END

-- 7. 清理 SystemSettings（移除定价相关 key，改名门店相关 key）
DELETE FROM SystemSettings WHERE [Key] IN (
    'holiday_pricing_enabled', 'base_hourly_rate',
    'room_type_multiplier_vip', 'room_type_multiplier_medium', 'room_type_multiplier_small',
    'store_address', 'business_hours',
    'verify_delete_order', 'verify_balance_adjust', 'verify_toggle_vip'
);

-- 改名 key
UPDATE SystemSettings SET [Key] = 'platform_name' WHERE [Key] = 'store_name';
UPDATE SystemSettings SET [Key] = 'contact_info' WHERE [Key] = 'store_phone';

-- 新增 key
IF NOT EXISTS (SELECT * FROM SystemSettings WHERE [Key] = 'verify_close_room')
    INSERT INTO SystemSettings ([Key], [Value]) VALUES ('verify_close_room', 'true');

-- 插入默认管理员账号（如不存在）
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, DisplayName, Role, Status, CreatedAt, UpdatedAt)
    VALUES ('admin', 'demo_hash_admin', N'管理员', 'admin', 'active', GETUTCDATE(), GETUTCDATE());
END

COMMIT TRANSACTION;

PRINT N'=== Migration v2.0 完成 ===';
PRINT N'Users 表新增 Email 字段';
PRINT N'Rooms 表重建为虚拟房间';
PRINT N'RoomRequests 表已创建';
PRINT N'Feedbacks 表已创建';
PRINT N'Orders 表已删除';
PRINT N'Holidays 表已删除';
PRINT N'SystemSettings 已清理';
