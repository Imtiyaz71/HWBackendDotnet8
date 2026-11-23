using Dapper;
using Som_Models.Models;
using Som_Service.Interface;

namespace Som_Service.Service
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserNotificationService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string type, int? relatedProblemId = null)
        {
            using var db = _connectionFactory.CreateConnection();

            var checkTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserNotifications' AND xtype='U')
                BEGIN
                    CREATE TABLE UserNotifications (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        Title NVARCHAR(200) NOT NULL,
                        Message NVARCHAR(MAX) NOT NULL,
                        Type NVARCHAR(50) NOT NULL,
                        RelatedProblemId INT NULL,
                        IsRead BIT DEFAULT 0,
                        CreatedAt DATETIME DEFAULT GETUTCDATE()
                    );
                END";

            await db.ExecuteAsync(checkTable);

            var sql = @"
                INSERT INTO UserNotifications (UserId, Title, Message, Type, RelatedProblemId, IsRead, CreatedAt)
                VALUES (@UserId, @Title, @Message, @Type, @RelatedProblemId, 0, GETUTCDATE())";

            await db.ExecuteAsync(sql, new { UserId = userId, Title = title, Message = message, Type = type, RelatedProblemId = relatedProblemId });
        }

        public async Task<List<UserNotification>> GetNotificationsAsync(int userId)
        {
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT * FROM UserNotifications 
                WHERE UserId = @UserId 
                ORDER BY CreatedAt DESC";

            var list = await db.QueryAsync<UserNotification>(sql, new { UserId = userId });
            return list.AsList();
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            using var db = _connectionFactory.CreateConnection();
            var rows = await db.ExecuteAsync("UPDATE UserNotifications SET IsRead = 1 WHERE Id = @Id", new { Id = notificationId });
            return rows > 0;
        }

        public async Task NotifySpecialistTeachersAsync(string subjectName, string title, string message, int problemId)
        {
            using var db = _connectionFactory.CreateConnection();

            // Find all teachers whose SpecialistSubjects contains this subject
            var sql = @"
                SELECT Id FROM RegisteredUsers 
                WHERE Role = 'Teacher' 
                  AND (SpecialistSubjects LIKE '%' + @SubjectName + '%' OR SpecialistSubjects IS NULL OR SpecialistSubjects = '[]')";

            var teacherIds = (await db.QueryAsync<int>(sql, new { SubjectName = subjectName })).ToList();

            foreach (var teacherId in teacherIds)
            {
                await CreateNotificationAsync(teacherId, title, message, "ProblemCreated", problemId);
            }
        }
    }
}
