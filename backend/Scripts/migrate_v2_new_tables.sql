-- =============================================
-- v2.0.0 新增表迁移脚本
-- 包含：RoomUsers、RoomRequests、Feedbacks
-- 执行前请确认 Rooms 和 Users 表已存在
-- =============================================

-- 1. RoomUsers 表（房间在线用户跟踪）
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RoomUsers')
BEGIN
    CREATE TABLE RoomUsers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoomId INT NOT NULL,
        UserId INT NOT NULL,
        JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_RoomUsers_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
        CONSTRAINT FK_RoomUsers_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    CREATE UNIQUE INDEX IX_RoomUsers_Room_User ON RoomUsers (RoomId, UserId);
    CREATE INDEX IX_RoomUsers_UserId ON RoomUsers (UserId);
    PRINT 'RoomUsers 表已创建';
END
ELSE
BEGIN
    PRINT 'RoomUsers 表已存在，跳过';
END

-- 2. RoomRequests 表（开房申请）
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RoomRequests')
BEGIN
    CREATE TABLE RoomRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
        RoomId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL,
        ProcessedBy INT NULL,
        CONSTRAINT FK_RoomRequests_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_RoomRequests_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
        CONSTRAINT FK_RoomRequests_ProcessedBy FOREIGN KEY (ProcessedBy) REFERENCES Users(Id)
    );
    CREATE INDEX IX_RoomRequests_Status ON RoomRequests (Status);
    CREATE INDEX IX_RoomRequests_UserId ON RoomRequests (UserId);
    PRINT 'RoomRequests 表已创建';
END
ELSE
BEGIN
    PRINT 'RoomRequests 表已存在，跳过';
END

-- 3. Feedbacks 表（用户反馈）
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Feedbacks')
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
        CONSTRAINT FK_Feedbacks_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    CREATE INDEX IX_Feedbacks_Status ON Feedbacks (Status);
    CREATE INDEX IX_Feedbacks_UserId ON Feedbacks (UserId);
    PRINT 'Feedbacks 表已创建';
END
ELSE
BEGIN
    PRINT 'Feedbacks 表已存在，跳过';
END

PRINT 'v2.0.0 迁移完成';
