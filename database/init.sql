-- ============================================
-- 声域友 KTV System — 数据库初始化脚本 v2.0
-- 适用数据库: SQL Server
-- 用法: 在 SQL Server Management Studio 或 sqlcmd 中执行此脚本
-- ============================================

-- 如果数据库不存在则创建
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KTVSystem')
BEGIN
    CREATE DATABASE KTVSystem;
    PRINT N'数据库 KTVSystem 已创建';
END
GO

USE KTVSystem;
GO

-- ============================================
-- 删除旧表（按外键依赖顺序）
-- ============================================
IF OBJECT_ID('dbo.ChatMessages', 'U') IS NOT NULL DROP TABLE ChatMessages;
IF OBJECT_ID('dbo.OperationLogs', 'U') IS NOT NULL DROP TABLE OperationLogs;
IF OBJECT_ID('dbo.Feedbacks', 'U') IS NOT NULL DROP TABLE Feedbacks;
IF OBJECT_ID('dbo.Favorites', 'U') IS NOT NULL DROP TABLE Favorites;
IF OBJECT_ID('dbo.PlayQueue', 'U') IS NOT NULL DROP TABLE PlayQueue;
IF OBJECT_ID('dbo.RoomUsers', 'U') IS NOT NULL DROP TABLE RoomUsers;
IF OBJECT_ID('dbo.RoomRequests', 'U') IS NOT NULL DROP TABLE RoomRequests;
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL DROP TABLE Rooms;
IF OBJECT_ID('dbo.Songs', 'U') IS NOT NULL DROP TABLE Songs;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE SystemSettings;
GO

-- ============================================
-- 建表
-- ============================================

-- 用户表
CREATE TABLE Users (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256) NOT NULL,
    DisplayName     NVARCHAR(100) NOT NULL,
    Phone           NVARCHAR(20) NULL,
    Email           NVARCHAR(100) NULL,
    AvatarUrl       NVARCHAR(500) NULL,
    Role            NVARCHAR(20) NOT NULL DEFAULT 'user',
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    LastActiveAt    DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;

-- 歌曲表
CREATE TABLE Songs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NOT NULL,
    Artist          NVARCHAR(200) NOT NULL,
    Genre           NVARCHAR(50) NOT NULL,
    Language        NVARCHAR(20) NULL,
    Duration        INT NOT NULL DEFAULT 0,
    FileSize        BIGINT NULL,
    CoverUrl        NVARCHAR(500) NULL,
    MediaUrl        NVARCHAR(500) NULL,
    OriginalFileName NVARCHAR(500) NULL,
    PlayCount       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_Songs_Genre ON Songs(Genre);
CREATE INDEX IX_Songs_Status ON Songs(Status);
CREATE INDEX IX_Songs_PlayCount ON Songs(PlayCount DESC);

-- 虚拟房间表
CREATE TABLE Rooms (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomCode        NVARCHAR(10) NOT NULL UNIQUE,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedByUserId INT NOT NULL,
    CurrentUsers    INT NOT NULL DEFAULT 0,
    IdleCloseAt     DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    ClosedAt        DATETIME2 NULL
);

-- 房间申请表
CREATE TABLE RoomRequests (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'pending',
    RoomId          INT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    ProcessedAt     DATETIME2 NULL,
    ProcessedBy     INT NULL
);

-- 房间用户关联表
CREATE TABLE RoomUsers (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL,
    UserId          INT NOT NULL,
    JoinedAt        DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_RoomUsers UNIQUE (UserId)
);

-- 播放队列表
CREATE TABLE PlayQueue (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL,
    SongId          INT NOT NULL,
    OrderedByUserId INT NOT NULL,
    SortOrder       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'queued',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_PlayQueue_RoomId_SortOrder ON PlayQueue(RoomId, SortOrder);

-- 收藏表
CREATE TABLE Favorites (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    SongId          INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Favorites_User_Song UNIQUE (UserId, SongId)
);
CREATE INDEX IX_Favorites_UserId ON Favorites(UserId);

-- 反馈表
CREATE TABLE Feedbacks (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    FeedbackType    NVARCHAR(30) NOT NULL,
    SongName        NVARCHAR(200) NULL,
    Artist          NVARCHAR(200) NULL,
    Description     NVARCHAR(1000) NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'pending',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    ProcessedAt     DATETIME2 NULL
);

-- 系统设置表
CREATE TABLE SystemSettings (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey      NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue    NVARCHAR(MAX) NOT NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 聊天消息表
CREATE TABLE ChatMessages (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL,
    UserId          INT NOT NULL,
    Nickname        NVARCHAR(100) NOT NULL,
    Content         NVARCHAR(500) NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_ChatMessages_RoomId ON ChatMessages(RoomId, CreatedAt);

-- 操作日志表
CREATE TABLE OperationLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL,
    OperationType   NVARCHAR(50) NOT NULL,
    ObjectType      NVARCHAR(50) NOT NULL,
    ObjectId        NVARCHAR(50) NULL,
    Details         NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_OperationLogs_CreatedAt ON OperationLogs(CreatedAt DESC);
CREATE INDEX IX_OperationLogs_Type ON OperationLogs(OperationType);

GO

-- ============================================
-- 种子数据
-- ============================================

-- 系统设置
INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES
    (N'platform_name', N'声域友 KTV'),
    (N'contact_info', N'admin@ktv.com'),
    (N'log_retention_days', N'90'),
    (N'verify_close_room', N'true');

-- 管理员账号（密码: admin123）
-- 注意: 这是明文存储的演示哈希，实际项目应使用 BCrypt 等算法
INSERT INTO Users (Username, PasswordHash, DisplayName, Role, Status, CreatedAt, UpdatedAt) VALUES
    (N'admin', N'admin123', N'管理员', N'admin', N'active', GETDATE(), GETDATE());

-- 示例用户（密码均为 123456）
INSERT INTO Users (Username, PasswordHash, DisplayName, Phone, Role, Status, CreatedAt, UpdatedAt) VALUES
    (N'zhangsan', N'123456', N'张三', N'13800000001', N'user', N'active', GETDATE(), GETDATE()),
    (N'lisi',     N'123456', N'李四', N'13800000002', N'user', N'active', GETDATE(), GETDATE());

-- 示例歌曲（音乐文件需放入 backend/wwwroot/uploads/music/ 目录）
INSERT INTO Songs (Title, Artist, Genre, Language, Duration, FileSize, CoverUrl, MediaUrl, OriginalFileName, PlayCount, Status, CreatedAt) VALUES
    (N'魂牵梦绕想着你',     N'倪尔萍',           N'流行', N'中文', 203, 3281633, N'/uploads/covers/default.jpg', N'/uploads/music/倪尔萍 - 魂牵梦绕想着你 [mqms].mp3', N'倪尔萍 - 魂牵梦绕想着你 [mqms].mp3', 0, N'active', GETDATE()),
    (N'老公最好',           N'弓秀丽',           N'流行', N'中文', 219, 3513177, N'/uploads/covers/default.jpg', N'/uploads/music/弓秀丽 - 老公最好 [mqms].mp3', N'弓秀丽 - 老公最好 [mqms].mp3', 0, N'active', GETDATE()),
    (N'爱到最后就是痛',     N'涓子&落叶摇情',    N'流行', N'中文', 220, 3534914, N'/uploads/covers/default.jpg', N'/uploads/music/涓子&落叶摇情 - 爱到最后就是痛 [mqms].mp3', N'涓子&落叶摇情 - 爱到最后就是痛 [mqms].mp3', 0, N'active', GETDATE()),
    (N'你的眼角流着我的泪', N'王韵',             N'流行', N'中文', 216, 3458856, N'/uploads/covers/default.jpg', N'/uploads/music/王韵 - 你的眼角流着我的泪 [mqms2].mp3', N'王韵 - 你的眼角流着我的泪 [mqms2].mp3', 0, N'active', GETDATE()),
    (N'一分不是爱，一分是伤害', N'网络歌手',       N'流行', N'中文', 191, 3058035, N'/uploads/covers/default.jpg', N'/uploads/music/网络歌手 - 一分不是爱，一分是伤害 [mqms].mp3', N'网络歌手 - 一分不是爱，一分是伤害 [mqms].mp3', 0, N'active', GETDATE()),
    (N'爱我是你说的谎',     N'项泽云',           N'流行', N'中文', 214, 3428775, N'/uploads/covers/default.jpg', N'/uploads/music/项泽云 - 爱我是你说的谎 (Live) [mqms].mp3', N'项泽云 - 爱我是你说的谎 (Live) [mqms].mp3', 0, N'active', GETDATE());

GO

PRINT N'============================================';
PRINT N'数据库初始化完成!';
PRINT N'';
PRINT N'表: Users, Songs, Rooms, RoomRequests, RoomUsers,';
PRINT N'     PlayQueue, Favorites, Feedbacks, SystemSettings,';
PRINT N'     ChatMessages, OperationLogs';
PRINT N'';
PRINT N'管理员账号: admin / admin123';
PRINT N'示例用户: zhangsan / 123456, lisi / 123456';
PRINT N'示例歌曲: 6 首（需确保 MP3 文件在 wwwroot/uploads/music/ 目录下）';
PRINT N'============================================';
