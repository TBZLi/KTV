-- ============================================
-- 声域友 KTV System - Database Schema
-- SQL Server / Dapper
-- ============================================

-- Users table (customers + admins)
CREATE TABLE Users (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    DisplayName     NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(20) NULL,
    Balance         DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    IsVip           BIT NOT NULL DEFAULT 0,
    Role            NVARCHAR(20) NOT NULL DEFAULT 'user',
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Rooms table
CREATE TABLE Rooms (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber      NVARCHAR(20) NOT NULL UNIQUE,
    RoomType        NVARCHAR(20) NOT NULL,
    Capacity        INT NOT NULL DEFAULT 6,
    HourlyRate      DECIMAL(10, 2) NOT NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'idle',
    CurrentOrderId  NVARCHAR(20) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Songs table
CREATE TABLE Songs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NOT NULL,
    Artist          NVARCHAR(200) NOT NULL,
    Genre           NVARCHAR(50) NOT NULL,
    Duration        INT NOT NULL,
    CoverUrl        NVARCHAR(500) NULL,
    MediaUrl        NVARCHAR(500) NULL,
    PlayCount       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Orders table
CREATE TABLE Orders (
    Id              NVARCHAR(20) NOT NULL PRIMARY KEY,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    RoomId          INT NOT NULL FOREIGN KEY REFERENCES Rooms(Id),
    OrderType       NVARCHAR(20) NOT NULL DEFAULT 'room',
    SongId          INT NULL FOREIGN KEY REFERENCES Songs(Id),
    Amount          DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'in_progress',
    StartTime       DATETIME2 NULL,
    EndTime         DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Play Queue
CREATE TABLE PlayQueue (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL FOREIGN KEY REFERENCES Rooms(Id),
    SongId          INT NOT NULL FOREIGN KEY REFERENCES Songs(Id),
    OrderedByUserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    SortOrder       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'queued',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Favorites
CREATE TABLE Favorites (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    SongId          INT NOT NULL FOREIGN KEY REFERENCES Songs(Id),
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Favorites_User_Song UNIQUE (UserId, SongId)
);

-- System Settings
CREATE TABLE SystemSettings (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey      NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue    NVARCHAR(MAX) NOT NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Holidays (dynamic pricing)
CREATE TABLE Holidays (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    StartDate       DATE NOT NULL,
    EndDate         DATE NOT NULL,
    VipMultiplier   DECIMAL(4, 2) NOT NULL DEFAULT 1.50,
    MediumMultiplier DECIMAL(4, 2) NOT NULL DEFAULT 1.30,
    SmallMultiplier DECIMAL(4, 2) NOT NULL DEFAULT 1.20,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Operation Logs
CREATE TABLE OperationLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL,
    OperationType   NVARCHAR(50) NOT NULL,
    ObjectType      NVARCHAR(50) NOT NULL,
    ObjectId        NVARCHAR(50) NULL,
    Details         NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Performance indexes
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_RoomId ON Orders(RoomId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);
CREATE INDEX IX_Songs_Genre ON Songs(Genre);
CREATE INDEX IX_Songs_Status ON Songs(Status);
CREATE INDEX IX_Songs_PlayCount ON Songs(PlayCount DESC);
CREATE INDEX IX_PlayQueue_RoomId_SortOrder ON PlayQueue(RoomId, SortOrder);
CREATE INDEX IX_Favorites_UserId ON Favorites(UserId);
CREATE INDEX IX_Rooms_Status ON Rooms(Status);
CREATE INDEX IX_Holidays_Dates ON Holidays(StartDate, EndDate);
CREATE INDEX IX_OperationLogs_CreatedAt ON OperationLogs(CreatedAt DESC);
CREATE INDEX IX_OperationLogs_Type ON OperationLogs(OperationType);

-- Default system settings
INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES
    ('store_name', N'声域友 KTV (旗舰店)'),
    ('store_phone', N'010-88888888'),
    ('store_address', N'北京市朝阳区建国路88号'),
    ('business_hours', N'14:00 - 02:00'),
    ('holiday_pricing_enabled', N'true'),
    ('base_hourly_rate', N'120'),
    ('log_retention_days', N'90'),
    ('sensitive_op_verification', N'false'),
    ('verify_delete_order', N'true'),
    ('verify_balance_adjust', N'true'),
    ('verify_disable_user', N'true'),
    ('verify_batch_song_status', N'true'),
    ('verify_modify_settings', N'true'),
    ('verify_modify_admin', N'true');

-- Sample data: Admin user (password: admin123)
INSERT INTO Users (Username, PasswordHash, DisplayName, Role, Status) VALUES
    ('admin', 'AQAAAAIAAYagAAAAEKj3P7...', N'系统管理员', 'admin', 'active');

-- Sample rooms
INSERT INTO Rooms (RoomNumber, RoomType, Capacity, HourlyRate, Status) VALUES
    ('V-001', 'VIP', 12, 288.00, 'idle'),
    ('V-002', 'VIP', 12, 288.00, 'idle'),
    ('V-003', 'VIP', 10, 288.00, 'idle'),
    ('V-004', 'VIP', 10, 288.00, 'idle'),
    ('V-005', 'VIP', 8, 288.00, 'idle'),
    ('M-001', 'Medium', 8, 168.00, 'idle'),
    ('M-002', 'Medium', 8, 168.00, 'idle'),
    ('M-003', 'Medium', 6, 168.00, 'idle'),
    ('M-004', 'Medium', 6, 168.00, 'idle'),
    ('M-005', 'Medium', 6, 168.00, 'idle'),
    ('M-006', 'Medium', 6, 168.00, 'idle'),
    ('M-007', 'Medium', 6, 168.00, 'idle'),
    ('M-008', 'Medium', 6, 168.00, 'idle'),
    ('M-009', 'Medium', 6, 168.00, 'idle'),
    ('S-001', 'Small', 4, 88.00, 'idle'),
    ('S-002', 'Small', 4, 88.00, 'idle'),
    ('S-003', 'Small', 4, 88.00, 'idle'),
    ('S-004', 'Small', 4, 88.00, 'idle'),
    ('S-005', 'Small', 4, 88.00, 'idle'),
    ('S-006', 'Small', 4, 88.00, 'idle'),
    ('S-007', 'Small', 4, 88.00, 'idle'),
    ('S-008', 'Small', 4, 88.00, 'idle'),
    ('S-009', 'Small', 4, 88.00, 'idle'),
    ('S-010', 'Small', 4, 88.00, 'idle');

-- Sample songs
INSERT INTO Songs (Title, Artist, Genre, Duration, PlayCount) VALUES
    (N'晴天', N'周杰伦', N'流行', 269, 999000),
    (N'起风了', N'买辣椒也用券', N'流行', 325, 850000),
    (N'孤勇者', N'陈奕迅', N'流行', 262, 720000),
    (N'稻香', N'周杰伦', N'流行', 223, 600000),
    (N'海阔天空', N'Beyond', N'摇滚', 326, 580000),
    (N'平凡之路', N'朴树', N'民谣', 295, 520000),
    (N'光年之外', N'邓紫棋', N'流行', 235, 480000),
    (N'夜曲', N'周杰伦', N'流行', 226, 450000),
    (N'红玫瑰', N'陈奕迅', N'流行', 264, 430000),
    (N'后来', N'刘若英', N'流行', 337, 410000),
    (N'倔强', N'五月天', N'摇滚', 264, 390000),
    (N'成都', N'赵雷', N'民谣', 329, 370000),
    (N'告白气球', N'周杰伦', N'流行', 215, 350000),
    (N'说散就散', N'袁娅维', N'R&B', 237, 330000),
    (N'南山南', N'马頔', N'民谣', 312, 310000),
    (N'体面', N'于文文', N'流行', 268, 290000),
    (N'消愁', N'毛不易', N'民谣', 315, 270000),
    (N'李白', N'李荣浩', N'流行', 264, 250000),
    (N'泡沫', N'邓紫棋', N'流行', 270, 230000),
    (N'无条件', N'陈奕迅', N'流行', 273, 210000);
