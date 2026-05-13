-- Fix song data encoding
DELETE FROM PlayQueue;
DELETE FROM Favorites;
DELETE FROM Songs;
SET IDENTITY_INSERT Songs ON;
INSERT INTO Songs (Id, Title, Artist, Genre, Duration, CoverUrl, PlayCount, Status, CreatedAt) VALUES
(1, N'晴天', N'周杰伦', N'流行', 269, N'', 999000, N'active', N'2023-01-15'),
(2, N'起风了', N'买辣椒也用券', N'流行', 325, N'', 850000, N'active', N'2023-01-20'),
(3, N'孤勇者', N'陈奕迅', N'流行', 262, N'', 720000, N'active', N'2023-02-01'),
(4, N'稻香', N'周杰伦', N'流行', 223, N'', 600000, N'active', N'2023-02-10'),
(5, N'海阔天空', N'Beyond', N'摇滚', 326, N'', 580000, N'active', N'2023-02-15'),
(6, N'平凡之路', N'朴树', N'民谣', 295, N'', 520000, N'active', N'2023-02-20'),
(7, N'光年之外', N'邓紫棋', N'流行', 235, N'', 480000, N'active', N'2023-03-01'),
(8, N'夜曲', N'周杰伦', N'流行', 226, N'', 450000, N'active', N'2023-03-05'),
(9, N'红玫瑰', N'陈奕迅', N'流行', 264, N'', 430000, N'active', N'2023-03-10'),
(10, N'后来', N'刘若英', N'流行', 337, N'', 410000, N'active', N'2023-03-15'),
(11, N'倔强', N'五月天', N'摇滚', 264, N'', 390000, N'active', N'2023-03-20'),
(12, N'成都', N'赵雷', N'民谣', 329, N'', 370000, N'active', N'2023-03-25'),
(13, N'告白气球', N'周杰伦', N'流行', 215, N'', 350000, N'active', N'2023-04-01'),
(14, N'说散就散', N'袁娅维', N'R&B', 237, N'', 330000, N'active', N'2023-04-05'),
(15, N'南山南', N'马頔', N'民谣', 312, N'', 310000, N'active', N'2023-04-10'),
(16, N'体面', N'于文文', N'流行', 268, N'', 290000, N'active', N'2023-04-15'),
(17, N'消愁', N'毛不易', N'民谣', 315, N'', 270000, N'active', N'2023-04-20'),
(18, N'李白', N'李荣浩', N'流行', 264, N'', 250000, N'active', N'2023-04-25'),
(19, N'泡沫', N'邓紫棋', N'流行', 270, N'', 230000, N'active', N'2023-05-01'),
(20, N'无条件', N'陈奕迅', N'流行', 273, N'', 210000, N'active', N'2023-05-05'),
(21, N'以父之名', N'周杰伦', N'嘻哈', 341, N'', 195000, N'active', N'2023-05-10'),
(22, N'富士山下', N'陈奕迅', N'流行', 284, N'', 180000, N'active', N'2023-05-15'),
(23, N'春风十里', N'鹿先森乐队', N'民谣', 305, N'', 165000, N'active', N'2023-05-20'),
(24, N'安河桥', N'宋冬野', N'民谣', 338, N'', 150000, N'active', N'2023-05-25'),
(25, N'玫瑰花的葬礼', N'许嵩', N'流行', 258, N'', 140000, N'active', N'2023-06-01');
SET IDENTITY_INSERT Songs OFF;

-- Re-insert favorites
INSERT INTO Favorites (UserId, SongId, CreatedAt) VALUES
(2, 1, '2023-10-20'), (2, 3, '2023-10-21'), (2, 5, '2023-10-22'),
(2, 6, '2023-10-23'), (2, 12, '2023-10-24'), (2, 8, '2023-10-24'),
(2, 14, '2023-10-25'), (2, 17, '2023-10-25');

-- Re-insert play queue (for room 1 = V-001)
INSERT INTO PlayQueue (RoomId, SongId, OrderedByUserId, SortOrder, Status, CreatedAt) VALUES
(1, 2, 2, 1, 'queued', '2023-10-26 14:00'),
(1, 3, 2, 2, 'queued', '2023-10-26 14:05'),
(1, 5, 3, 3, 'queued', '2023-10-26 14:10'),
(1, 7, 2, 4, 'queued', '2023-10-26 14:15'),
(1, 9, 3, 5, 'queued', '2023-10-26 14:20'),
(1, 11, 2, 6, 'queued', '2023-10-26 14:22'),
(1, 13, 3, 7, 'queued', '2023-10-26 14:25'),
(1, 15, 2, 8, 'queued', '2023-10-26 14:28'),
(1, 18, 3, 9, 'queued', '2023-10-26 14:30'),
(1, 20, 2, 10, 'queued', '2023-10-26 14:32'),
(1, 22, 3, 11, 'queued', '2023-10-26 14:35'),
(1, 24, 2, 12, 'queued', '2023-10-26 14:38');

-- Update room 1 status to in_use so queue shows up
UPDATE Rooms SET Status = 'in_use', CurrentOrderId = 'ORD-8924' WHERE Id = 1;
