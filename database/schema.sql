-- ============================================
-- 声域友 KTV System - Database Schema v2.0
-- SQL Server / Dapper
-- 注意: 此文件仅含建表 DDL，不含数据。完整初始化请用 init.sql
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
    Balance         DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    IsVip           BIT NOT NULL DEFAULT 0,
    Role            NVARCHAR(20) NOT NULL DEFAULT 'user',
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    LastActiveAt    DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

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
    LrcUrl          NVARCHAR(500) NULL,
    OriginalFileName NVARCHAR(500) NULL,
    PlayCount       INT NOT NULL DEFAULT 0,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 虚拟房间表
CREATE TABLE Rooms (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomCode        NVARCHAR(10) NOT NULL UNIQUE,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedByUserId INT NOT NULL,
    CurrentUsers    INT NOT NULL DEFAULT 0,
    IdleCloseAt     DATETIME2 NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ClosedAt        DATETIME2 NULL
);

-- 房间申请表
CREATE TABLE RoomRequests (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'pending',
    RoomId          INT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt     DATETIME2 NULL,
    ProcessedBy     INT NULL
);

-- 房间用户关联表
CREATE TABLE RoomUsers (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL,
    UserId          INT NOT NULL,
    JoinedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
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
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 收藏表
CREATE TABLE Favorites (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    SongId          INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Favorites_User_Song UNIQUE (UserId, SongId)
);

-- 反馈表
CREATE TABLE Feedbacks (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    FeedbackType    NVARCHAR(30) NOT NULL,
    SongName        NVARCHAR(200) NULL,
    Artist          NVARCHAR(200) NULL,
    Description     NVARCHAR(1000) NULL,
    Status          NVARCHAR(20) NOT NULL DEFAULT 'pending',
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt     DATETIME2 NULL
);

-- 系统设置表
CREATE TABLE SystemSettings (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey      NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue    NVARCHAR(MAX) NOT NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 聊天消息表
CREATE TABLE ChatMessages (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT NOT NULL,
    UserId          INT NOT NULL,
    Nickname        NVARCHAR(50) NOT NULL,
    Content         NVARCHAR(200) NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 操作日志表
CREATE TABLE OperationLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL,
    OperationType   NVARCHAR(50) NOT NULL,
    ObjectType      NVARCHAR(50) NOT NULL,
    ObjectId        NVARCHAR(50) NULL,
    Details         NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 性能索引
CREATE INDEX IX_Songs_Genre ON Songs(Genre);
CREATE INDEX IX_Songs_Status ON Songs(Status);
CREATE INDEX IX_Songs_PlayCount ON Songs(PlayCount DESC);
CREATE INDEX IX_PlayQueue_RoomId_SortOrder ON PlayQueue(RoomId, SortOrder);
CREATE INDEX IX_Favorites_UserId ON Favorites(UserId);
CREATE INDEX IX_ChatMessages_RoomId ON ChatMessages(RoomId, CreatedAt);
CREATE INDEX IX_OperationLogs_CreatedAt ON OperationLogs(CreatedAt DESC);
CREATE INDEX IX_OperationLogs_Type ON OperationLogs(OperationType);
