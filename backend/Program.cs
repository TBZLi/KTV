using Dapper;
using backend.Hubs;
using backend.Middleware;
using backend.Repositories;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// File upload size limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB max (music files)
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS;Database=KTVSystem;Trusted_Connection=true;TrustServerCertificate=true;";
builder.Services.AddSingleton(connectionString);

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomRequestRepository, RoomRequestRepository>();
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IPlayQueueRepository, PlayQueueRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<IOperationLogRepository, OperationLogRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IRoomUserRepository, RoomUserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<RoomRequestService>();
builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<FeedbackService>();

// SignalR
builder.Services.AddSignalR();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ktv-system",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ktv-client",
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere12345678901234"))
        };
    });

var app = builder.Build();

// Auto-migration: ensure schema is up to date
{
    using var scope = app.Services.CreateScope();
    var connStr = scope.ServiceProvider.GetRequiredService<string>();
    using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
    await conn.OpenAsync();

    // Helper: check if a column exists
    bool ColumnExists(string table, string column) =>
        conn.ExecuteScalar<int>(
            $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @T AND COLUMN_NAME = @C",
            new { T = table, C = column }) > 0;
    bool TableExists(string table) =>
        conn.ExecuteScalar<int>(
            $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @T",
            new { T = table }) > 0;

    // 1. Users table: add Email column
    if (!ColumnExists("Users", "Email"))
    {
        conn.Execute("ALTER TABLE Users ADD Email NVARCHAR(100) NULL");
        try { conn.Execute("ALTER TABLE Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email)"); } catch { }
        Console.WriteLine("[AutoMigrate] Users.Email column added");
    }

    // 2. Rooms table: rebuild if old schema detected
    if (ColumnExists("Rooms", "RoomNumber") && !ColumnExists("Rooms", "RoomCode"))
    {
        // Old schema found — drop FK constraints referencing Rooms, then drop and recreate
        var fkConstraints = conn.Query<string>(
            @"SELECT fk.name FROM sys.foreign_keys fk
              INNER JOIN sys.tables t ON fk.referenced_object_id = t.object_id
              WHERE t.name = 'Rooms'").ToList();
        foreach (var fk in fkConstraints)
        {
            // Find the parent table
            var parentTable = conn.QuerySingleOrDefault<string>(
                @"SELECT t.name FROM sys.foreign_keys fk
                  INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
                  WHERE fk.name = @Fk", new { Fk = fk });
            if (parentTable != null)
                conn.Execute($"ALTER TABLE [{parentTable}] DROP CONSTRAINT [{fk}]");
        }
        conn.Execute("DROP TABLE Rooms");
        conn.Execute(@"
            CREATE TABLE Rooms (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RoomCode NVARCHAR(10) NOT NULL UNIQUE,
                Status NVARCHAR(20) NOT NULL DEFAULT 'active',
                CreatedByUserId INT NOT NULL,
                CurrentUsers INT NOT NULL DEFAULT 0,
                IdleCloseAt DATETIME2 NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                ClosedAt DATETIME2 NULL
            )");
        Console.WriteLine("[AutoMigrate] Rooms table rebuilt with new schema");
    }
    else if (!TableExists("Rooms"))
    {
        conn.Execute(@"
            CREATE TABLE Rooms (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RoomCode NVARCHAR(10) NOT NULL UNIQUE,
                Status NVARCHAR(20) NOT NULL DEFAULT 'active',
                CreatedByUserId INT NOT NULL,
                CurrentUsers INT NOT NULL DEFAULT 0,
                IdleCloseAt DATETIME2 NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                ClosedAt DATETIME2 NULL
            )");
        Console.WriteLine("[AutoMigrate] Rooms table created");
    }

    // 3. RoomRequests table
    if (!TableExists("RoomRequests"))
    {
        conn.Execute(@"
            CREATE TABLE RoomRequests (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NOT NULL,
                Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
                RoomId INT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                ProcessedAt DATETIME2 NULL,
                ProcessedBy INT NULL
            )");
        Console.WriteLine("[AutoMigrate] RoomRequests table created");
    }

    // 4. Feedbacks table
    if (!TableExists("Feedbacks"))
    {
        conn.Execute(@"
            CREATE TABLE Feedbacks (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NOT NULL,
                FeedbackType NVARCHAR(30) NOT NULL,
                SongName NVARCHAR(200) NULL,
                Artist NVARCHAR(200) NULL,
                Description NVARCHAR(1000) NULL,
                Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
                CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                ProcessedAt DATETIME2 NULL
            )");
        Console.WriteLine("[AutoMigrate] Feedbacks table created");
    }

    // 5. PlayQueue table
    if (!TableExists("PlayQueue"))
    {
        conn.Execute(@"
            CREATE TABLE PlayQueue (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RoomId INT NOT NULL,
                SongId INT NOT NULL,
                OrderedByUserId INT NOT NULL,
                SortOrder INT NOT NULL DEFAULT 0,
                Status NVARCHAR(20) NOT NULL DEFAULT 'queued',
                CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
            )");
        Console.WriteLine("[AutoMigrate] PlayQueue table created");
    }

    // 6. RoomUsers table (tracks active room membership)
    if (!TableExists("RoomUsers"))
    {
        conn.Execute(@"
            CREATE TABLE RoomUsers (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RoomId INT NOT NULL,
                UserId INT NOT NULL,
                JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                CONSTRAINT UQ_RoomUsers UNIQUE (UserId)
            )");
        Console.WriteLine("[AutoMigrate] RoomUsers table created");
    }

    // 7. Users table: add LastActiveAt column
    if (!ColumnExists("Users", "LastActiveAt"))
    {
        conn.Execute("ALTER TABLE Users ADD LastActiveAt DATETIME2 NULL");
        Console.WriteLine("[AutoMigrate] Users.LastActiveAt column added");
    }

    // 8. Songs table: add OriginalFileName column
    if (!ColumnExists("Songs", "OriginalFileName"))
    {
        conn.Execute("ALTER TABLE Songs ADD OriginalFileName NVARCHAR(500) NULL");
        Console.WriteLine("[AutoMigrate] Songs.OriginalFileName column added");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontends");

// Serve static files from wwwroot (for uploaded avatars, covers, music)
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".flac"] = "audio/flac";
contentTypeProvider.Mappings[".lrc"] = "text/plain";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypeProvider });

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UpdateLastActiveMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<OperationLoggingMiddleware>();
app.MapControllers();
app.MapHub<KtvHub>("/hubs/ktv");
app.Run();
