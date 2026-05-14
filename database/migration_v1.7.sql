-- ============================================
-- 声域友 KTV System - Migration v1.7
-- Songs 表新增 Language 和 FileSize 字段
-- ============================================

-- 1. Songs 表添加 Language 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Songs') AND name = 'Language')
BEGIN
    ALTER TABLE Songs ADD Language NVARCHAR(20) NULL;
    -- 将现有歌曲默认设为中文
    UPDATE Songs SET Language = N'中文' WHERE Language IS NULL;
END

-- 2. Songs 表添加 FileSize 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Songs') AND name = 'FileSize')
BEGIN
    ALTER TABLE Songs ADD FileSize BIGINT NULL;
END

PRINT N'=== Migration v1.7 完成 ===';
PRINT N'Songs 表新增 Language 和 FileSize 字段';
