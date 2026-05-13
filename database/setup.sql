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
    ('log_retention_days', N'90'),
    ('sensitive_op_verification', N'false');

-- Users (12 customers + 1 admin, password all: 123456)
-- Using a simple hash for demo purposes
INSERT INTO Users (Username, PasswordHash, DisplayName, Phone, Balance, IsVip, Role, Status, CreatedAt) VALUES
    ('admin',    'demo_hash_admin',    N'系统管理员', '13800000000', 0,    0, 'admin', 'active', '2023-10-15 10:00'),
    ('user_001', 'demo_hash', N'张三',   '13800000001', 500,   0, 'user',  'active',   '2023-10-25 10:00'),
    ('VIP_002',  'demo_hash', N'李四',   '13800000002', 1200,  1, 'user',  'active',   '2023-10-25 11:30'),
    ('test_003', 'demo_hash', N'王五',   '13800000003', 0,     0, 'user',  'disabled', '2023-10-24 15:45'),
    ('user_004', 'demo_hash', N'赵六',   '13800000004', 350,   0, 'user',  'active',   '2023-10-23 09:15'),
    ('VIP_005',  'demo_hash', N'钱七',   '13800000005', 2800,  1, 'user',  'active',   '2023-10-22 14:20'),
    ('user_006', 'demo_hash', N'孙八',   '13800000006', 180,   0, 'user',  'active',   '2023-10-21 16:30'),
    ('user_007', 'demo_hash', N'周九',   '13800000007', 0,     0, 'user',  'disabled', '2023-10-20 11:00'),
    ('VIP_008',  'demo_hash', N'吴十',   '13800000008', 950,   1, 'user',  'active',   '2023-10-19 13:45'),
    ('user_009', 'demo_hash', N'郑冬',   '13800000009', 420,   0, 'user',  'active',   '2023-10-18 10:30'),
    ('user_010', 'demo_hash', N'冯雪',   '13800000010', 75,    0, 'user',  'active',   '2023-10-17 08:20'),
    ('user_011', 'demo_hash', N'陈风',   '13800000011', 610,   0, 'user',  'active',   '2023-10-16 17:10'),
    ('VIP_012',  'demo_hash', N'楚云',   '13800000012', 3200,  1, 'user',  'active',   '2023-10-15 12:00');

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

-- Songs (25 songs, matching mock data exactly)
INSERT INTO Songs (Title, Artist, Genre, Duration, CoverUrl, PlayCount, Status, CreatedAt) VALUES
    (N'晴天',         N'周杰伦',       N'流行', 269, '', 999000, 'active', '2023-01-15'),
    (N'起风了',       N'买辣椒也用券', N'流行', 325, '', 850000, 'active', '2023-01-20'),
    (N'孤勇者',       N'陈奕迅',       N'流行', 262, '', 720000, 'active', '2023-02-01'),
    (N'稻香',         N'周杰伦',       N'流行', 223, '', 600000, 'active', '2023-02-10'),
    (N'海阔天空',     N'Beyond',       N'摇滚', 326, '', 580000, 'active', '2023-02-15'),
    (N'平凡之路',     N'朴树',         N'民谣', 295, '', 520000, 'active', '2023-02-20'),
    (N'光年之外',     N'邓紫棋',       N'流行', 235, '', 480000, 'active', '2023-03-01'),
    (N'夜曲',         N'周杰伦',       N'流行', 226, '', 450000, 'active', '2023-03-05'),
    (N'红玫瑰',       N'陈奕迅',       N'流行', 264, '', 430000, 'active', '2023-03-10'),
    (N'后来',         N'刘若英',       N'流行', 337, '', 410000, 'active', '2023-03-15'),
    (N'倔强',         N'五月天',       N'摇滚', 264, '', 390000, 'active', '2023-03-20'),
    (N'成都',         N'赵雷',         N'民谣', 329, '', 370000, 'active', '2023-03-25'),
    (N'告白气球',     N'周杰伦',       N'流行', 215, '', 350000, 'active', '2023-04-01'),
    (N'说散就散',     N'袁娅维',       N'R&B',  237, '', 330000, 'active', '2023-04-05'),
    (N'南山南',       N'马頔',         N'民谣', 312, '', 310000, 'active', '2023-04-10'),
    (N'体面',         N'于文文',       N'流行', 268, '', 290000, 'active', '2023-04-15'),
    (N'消愁',         N'毛不易',       N'民谣', 315, '', 270000, 'active', '2023-04-20'),
    (N'李白',         N'李荣浩',       N'流行', 264, '', 250000, 'active', '2023-04-25'),
    (N'泡沫',         N'邓紫棋',       N'流行', 270, '', 230000, 'active', '2023-05-01'),
    (N'无条件',       N'陈奕迅',       N'流行', 273, '', 210000, 'active', '2023-05-05'),
    (N'以父之名',     N'周杰伦',       N'嘻哈', 341, '', 195000, 'active', '2023-05-10'),
    (N'富士山下',     N'陈奕迅',       N'流行', 284, '', 180000, 'active', '2023-05-15'),
    (N'春风十里',     N'鹿先森乐队',   N'民谣', 305, '', 165000, 'active', '2023-05-20'),
    (N'安河桥',       N'宋冬野',       N'民谣', 338, '', 150000, 'active', '2023-05-25'),
    (N'玫瑰花的葬礼', N'许嵩',         N'流行', 258, '', 140000, 'active', '2023-06-01');

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

-- PlayQueue (12 songs queued in room 1)
INSERT INTO PlayQueue (RoomId, SongId, OrderedByUserId, SortOrder, Status, CreatedAt) VALUES
    (1, 2,  2, 1,  'queued', '2023-10-26 14:00'),
    (1, 3,  2, 2,  'queued', '2023-10-26 14:05'),
    (1, 5,  3, 3,  'queued', '2023-10-26 14:10'),
    (1, 7,  2, 4,  'queued', '2023-10-26 14:15'),
    (1, 9,  3, 5,  'queued', '2023-10-26 14:20'),
    (1, 11, 2, 6,  'queued', '2023-10-26 14:22'),
    (1, 13, 3, 7,  'queued', '2023-10-26 14:25'),
    (1, 15, 2, 8,  'queued', '2023-10-26 14:28'),
    (1, 18, 3, 9,  'queued', '2023-10-26 14:30'),
    (1, 20, 2, 10, 'queued', '2023-10-26 14:32'),
    (1, 22, 3, 11, 'queued', '2023-10-26 14:35'),
    (1, 24, 2, 12, 'queued', '2023-10-26 14:38');

-- Favorites (user 2 = 张三's favorites)
INSERT INTO Favorites (UserId, SongId, CreatedAt) VALUES
    (2, 1,  '2023-10-20'),
    (2, 3,  '2023-10-21'),
    (2, 5,  '2023-10-22'),
    (2, 6,  '2023-10-23'),
    (2, 12, '2023-10-24'),
    (2, 8,  '2023-10-24'),
    (2, 14, '2023-10-25'),
    (2, 17, '2023-10-25');

PRINT N'=== 数据库初始化完成 ===';
PRINT N'Tables: Users, Rooms, Songs, Orders, PlayQueue, Favorites, SystemSettings';
PRINT N'Sample data inserted successfully.';
