-- 添加 Email 列到 Users 表（如果不存在）
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Email'
)
BEGIN
    ALTER TABLE Users ADD Email NVARCHAR(100) NULL;
    ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
    PRINT 'Email 列已添加到 Users 表';
END
ELSE
BEGIN
    PRINT 'Email 列已存在，跳过';
END
