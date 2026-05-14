-- ============================================
-- 声域友 KTV System - Migration v1.6
-- 替换测试歌曲为真实歌曲 + 设置默认头像/封面
-- ============================================

-- 0. 给 Users 表添加 AvatarUrl 字段（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD AvatarUrl NVARCHAR(500) NULL;
END

-- 1. 清空依赖歌曲的关联表（旧测试歌曲 ID 将被清除）
DELETE FROM Favorites;
DELETE FROM PlayQueue;

-- 2. 清空旧测试歌曲
DELETE FROM Songs;

-- 3. 插入真实歌曲（封面统一用 default.jpg，音乐路径指向实际文件）
INSERT INTO Songs (Title, Artist, Genre, Duration, CoverUrl, MediaUrl, PlayCount, Status, CreatedAt) VALUES
    (N'魂牵梦绕想着你',     N'倪尔萍',           N'流行', 203, '/uploads/covers/default.jpg', '/uploads/music/倪尔萍 - 魂牵梦绕想着你 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'老公最好',           N'弓秀丽',           N'流行', 219, '/uploads/covers/default.jpg', '/uploads/music/弓秀丽 - 老公最好 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'爱到最后就是痛',     N'涓子&落叶摇情',    N'流行', 220, '/uploads/covers/default.jpg', '/uploads/music/涓子&落叶摇情 - 爱到最后就是痛 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'你的眼角流着我的泪', N'王韵',             N'流行', 216, '/uploads/covers/default.jpg', '/uploads/music/王韵 - 你的眼角流着我的泪 [mqms2].mp3', 0, 'active', GETUTCDATE()),
    (N'一分不是爱，一分是伤害', N'网络歌手',       N'流行', 191, '/uploads/covers/default.jpg', '/uploads/music/网络歌手 - 一分不是爱，一分是伤害 [mqms].mp3', 0, 'active', GETUTCDATE()),
    (N'爱我是你说的谎',     N'项泽云',           N'流行', 214, '/uploads/covers/default.jpg', '/uploads/music/项泽云 - 爱我是你说的谎 (Live) [mqms].mp3', 0, 'active', GETUTCDATE());

-- 4. 所有用户设置默认头像
UPDATE Users SET AvatarUrl = '/uploads/avatars/default.jpg';

PRINT N'=== Migration v1.6 完成 ===';
PRINT N'歌曲已替换为 6 首真实歌曲';
PRINT N'用户头像已设置为 default.jpg';
