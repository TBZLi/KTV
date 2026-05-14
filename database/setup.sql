-- ============================================
-- 声域友 KTV System - Complete Database Setup
-- SQL Server / Dapper
-- Run on: KTVSystem database
-- ============================================

-- Drop existing tables (in correct FK order)
IF OBJECT_ID('dbo.Favorites', 'U') IS NOT NULL DROP TABLE Favorites;
IF OBJECT_ID('dbo.PlayQueue', 'U') IS NOT NULL DROP TABLE PlayQueue;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('dbo.Songs', 'U') IS NOT NULL DROP TABLE Songs;
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL DROP TABLE Rooms;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE SystemSettings;

-- ============================================
-- Table Definitions
-- ============================================

CREATE TABLE Users (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    DisplayName     NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(20) NULL,
    AvatarUrl       NVARCHAR(500) NULL,
    Balance         DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    IsVip           BIT NOT NULL DEFAULT 0,
    Role            NVARCHAR(20) NOT NULL DEFAULT 'user',
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

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

CREATE TABLE PlayQueue (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL FOREIGN KEY REFERENCES Rooms(Id),
    SongId          INT NOT NULL FOREIGN KEY REFERENCES Songs(Id),
    OrderedByUserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    SortOrder       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'queued',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Favorites (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    SongId          INT NOT NULL FOREIGN KEY REFERENCES Songs(Id),
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Favorites_User_Song UNIQUE (UserId, SongId)
);

CREATE TABLE SystemSettings (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey      NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue    NVARCHAR(MAX) NOT NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================
-- Indexes
-- ============================================
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

-- ============================================
-- Seed Data
-- ============================================

-- System Settings
INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES
    ('store_name', N'声域友 KTV (旗舰店)'),
    ('store_phone', N'010-88888888'),
    ('store_address', N'北京市朝阳区建国路88号'),
    ('business_hours', N'14:00 - 02:00'),
    ('holiday_pricing_enabled', N'true'),
    ('base_hourly_rate', N'120'),
    ('room_type_multiplier_vip', N'1.5'),
    ('room_type_multiplier_medium', N'1.3'),
    ('room_type_multiplier_small', N'1.0'),
    ('log_retention_days', N'90'),
    ('sensitive_op_verification', N'false');

-- Users (12 customers + 1 admin, password all: 123456)
-- Using a simple hash for demo purposes
INSERT INTO Users (Username, PasswordHash, DisplayName, Phone, AvatarUrl, Balance, IsVip, Role, Status, CreatedAt) VALUES
    ('admin',    'demo_hash_admin',    N'系统管理员', '13800000000', '/uploads/avatars/default.jpg', 0,    0, 'admin', 'active', '2023-10-15 10:00'),
    ('user_001', 'demo_hash', N'张三',   '13800000001', '/uploads/avatars/default.jpg', 500,   0, 'user',  'active',   '2023-10-25 10:00'),
    ('VIP_002',  'demo_hash', N'李四',   '13800000002', '/uploads/avatars/default.jpg', 1200,  1, 'user',  'active',   '2023-10-25 11:30'),
    ('test_003', 'demo_hash', N'王五',   '13800000003', '/uploads/avatars/default.jpg', 0,     0, 'user',  'disabled', '2023-10-24 15:45'),
    ('user_004', 'demo_hash', N'赵六',   '13800000004', '/uploads/avatars/default.jpg', 350,   0, 'user',  'active',   '2023-10-23 09:15'),
    ('VIP_005',  'demo_hash', N'钱七',   '13800000005', '/uploads/avatars/default.jpg', 2800,  1, 'user',  'active',   '2023-10-22 14:20'),
    ('user_006', 'demo_hash', N'孙八',   '13800000006', '/uploads/avatars/default.jpg', 180,   0, 'user',  'active',   '2023-10-21 16:30'),
    ('user_007', 'demo_hash', N'周九',   '13800000007', '/uploads/avatars/default.jpg', 0,     0, 'user',  'disabled', '2023-10-20 11:00'),
    ('VIP_008',  'demo_hash', N'吴十',   '13800000008', '/uploads/avatars/default.jpg', 950,   1, 'user',  'active',   '2023-10-19 13:45'),
    ('user_009', 'demo_hash', N'郑冬',   '13800000009', '/uploads/avatars/default.jpg', 420,   0, 'user',  'active',   '2023-10-18 10:30'),
    ('user_010', 'demo_hash', N'冯雪',   '13800000010', '/uploads/avatars/default.jpg', 75,    0, 'user',  'active',   '2023-10-17 08:20'),
    ('user_011', 'demo_hash', N'陈风',   '13800000011', '/uploads/avatars/default.jpg', 610,   0, 'user',  'active',   '2023-10-16 17:10'),
    ('VIP_012',  'demo_hash', N'楚云',   '13800000012', '/uploads/avatars/default.jpg', 3200,  1, 'user',  'active',   '2023-10-15 12:00');

-- Rooms (24 rooms: 5 VIP + 9 Medium + 10 Small)
INSERT INTO Rooms (RoomNumber, RoomType, Capacity, HourlyRate, Status, CurrentOrderId) VALUES
    ('V-001', 'VIP',    12, 288.00, 'in_use',   'ORD-8924'),
    ('V-002', 'VIP',    12, 288.00, 'cleaning', NULL),
    ('V-003', 'VIP',    10, 288.00, 'idle',     NULL),
    ('V-004', 'VIP',    10, 288.00, 'in_use',   'ORD-8916'),
    ('V-005', 'VIP',    8,  288.00, 'idle',     NULL),
    ('M-001', 'Medium', 8,  168.00, 'in_use',   'ORD-8923'),
    ('M-002', 'Medium', 8,  168.00, 'in_use',   'ORD-8921'),
    ('M-003', 'Medium', 6,  168.00, 'idle',     NULL),
    ('M-004', 'Medium', 6,  168.00, 'idle',     NULL),
    ('M-005', 'Medium', 6,  168.00, 'cleaning', NULL),
    ('M-006', 'Medium', 6,  168.00, 'in_use',   'ORD-8919'),
    ('M-007', 'Medium', 6,  168.00, 'idle',     NULL),
    ('M-008', 'Medium', 6,  168.00, 'in_use',   'ORD-8915'),
    ('M-009', 'Medium', 6,  168.00, 'idle',     NULL),
    ('S-001', 'Small',  4,  88.00,  'in_use',   'ORD-8920'),
    ('S-002', 'Small',  4,  88.00,  'idle',     NULL),
    ('S-003', 'Small',  4,  88.00,  'in_use',   'ORD-8918'),
    ('S-004', 'Small',  4,  88.00,  'idle',     NULL),
    ('S-005', 'Small',  4,  88.00,  'in_use',   'ORD-8917'),
    ('S-006', 'Small',  4,  88.00,  'cleaning', NULL),
    ('S-007', 'Small',  4,  88.00,  'idle',     NULL),
    ('S-008', 'Small',  4,  88.00,  'idle',     NULL),
    ('S-009', 'Small',  4,  88.00,  'idle',     NULL),
    ('S-010', 'Small',  4,  88.00,  'in_use',   'ORD-8914');

-- Songs (6 songs with real files)
INSERT INTO Songs (Title, Artist, Genre, Duration, CoverUrl, MediaUrl, PlayCount, Status, CreatedAt) VALUES
    (N'魂牵梦绕想着你',     N'倪尔萍',           N'流行', 203, '/uploads/covers/default.jpg', N'/uploads/music/倪尔萍 - 魂牵梦绕想着你 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'老公最好',           N'弓秀丽',           N'流行', 219, '/uploads/covers/default.jpg', N'/uploads/music/弓秀丽 - 老公最好 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'爱到最后就是痛',     N'涓子&落叶摇情',    N'流行', 220, '/uploads/covers/default.jpg', N'/uploads/music/涓子&落叶摇情 - 爱到最后就是痛 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'你的眼角流着我的泪', N'王韵',             N'流行', 216, '/uploads/covers/default.jpg', N'/uploads/music/王韵 - 你的眼角流着我的泪 [mqms2].mp3', 0, 'active', GETUTCDATE()),
    (N'一分不是爱，一分是伤害', N'网络歌手',       N'流行', 191, '/uploads/covers/default.jpg', N'/uploads/music/网络歌手 - 一分不是爱，一分是伤害 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'爱我是你说的谎',     N'项泽云',           N'流行', 214, '/uploads/covers/default.jpg', N'/uploads/music/项泽云 - 爱我是你说的谎 (Live) [mqms].mp3', 0, 'active', GETUTCDATE());

-- Orders (15 orders, matching mock data)
INSERT INTO Orders (Id, UserId, RoomId, OrderType, Amount, Status, StartTime, CreatedAt) VALUES
    ('ORD-8924', 3,  1,  'room', 450.00,  'in_progress', '2023-10-26 14:30', '2023-10-26 14:30'),
    ('ORD-8923', 2,  6,  'room', 128.50,  'completed',   '2023-10-26 13:15', '2023-10-26 13:15'),
    ('ORD-8922', 4,  15, 'room', 65.00,   'cancelled',   '2023-10-26 12:00', '2023-10-26 12:00'),
    ('ORD-8921', 5,  7,  'room', 198.00,  'in_progress', '2023-10-26 11:45', '2023-10-26 11:45'),
    ('ORD-8920', 6,  15, 'room', 85.00,   'in_progress', '2023-10-26 11:30', '2023-10-26 11:30'),
    ('ORD-8919', 7,  11, 'room', 210.00,  'in_progress', '2023-10-26 10:00', '2023-10-26 10:00'),
    ('ORD-8918', 8,  17, 'room', 72.00,   'in_progress', '2023-10-26 09:30', '2023-10-26 09:30'),
    ('ORD-8917', 9,  19, 'room', 95.00,   'in_progress', '2023-10-26 09:00', '2023-10-26 09:00'),
    ('ORD-8916', 10, 4,  'room', 520.00,  'in_progress', '2023-10-26 08:30', '2023-10-26 08:30'),
    ('ORD-8915', 11, 13, 'room', 165.00,  'in_progress', '2023-10-26 08:00', '2023-10-26 08:00'),
    ('ORD-8914', 12, 24, 'room', 88.00,   'in_progress', '2023-10-26 07:30', '2023-10-26 07:30'),
    ('ORD-8913', 13, 1,  'room', 680.00,  'completed',   '2023-10-25 22:00', '2023-10-25 22:00'),
    ('ORD-8912', 2,  6,  'room', 245.00,  'completed',   '2023-10-25 20:30', '2023-10-25 20:30'),
    ('ORD-8911', 3,  8,  'room', 175.00,  'completed',   '2023-10-25 19:00', '2023-10-25 19:00'),
    ('ORD-8910', 4,  16, 'room', 55.00,   'cancelled',   '2023-10-25 18:00', '2023-10-25 18:00');

-- PlayQueue (6 songs queued in room 1)
INSERT INTO PlayQueue (RoomId, SongId, OrderedByUserId, SortOrder, Status, CreatedAt) VALUES
    (1, 1, 2, 1,  'queued', '2023-10-26 14:00'),
    (1, 2, 2, 2,  'queued', '2023-10-26 14:05'),
    (1, 3, 3, 3,  'queued', '2023-10-26 14:10'),
    (1, 4, 2, 4,  'queued', '2023-10-26 14:15'),
    (1, 5, 3, 5,  'queued', '2023-10-26 14:20'),
    (1, 6, 2, 6,  'queued', '2023-10-26 14:22');

-- Favorites (user 2 = 张三's favorites)
INSERT INTO Favorites (UserId, SongId, CreatedAt) VALUES
    (2, 1,  '2023-10-20'),
    (2, 3,  '2023-10-21'),
    (2, 5,  '2023-10-22'),
    (2, 6,  '2023-10-23');

PRINT N'=== 数据库初始化完成 ===';
PRINT N'Tables: Users, Rooms, Songs, Orders, PlayQueue, Favorites, SystemSettings';
PRINT N'Sample data inserted successfully.';
