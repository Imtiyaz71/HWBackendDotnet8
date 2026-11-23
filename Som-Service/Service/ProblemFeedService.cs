using Dapper;
using Som_Models.Models;
using Som_Service.Interface;

namespace Som_Service.Service
{
    public class ProblemFeedService : IProblemFeedService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IUserNotificationService _notificationService;

        public ProblemFeedService(IDbConnectionFactory connectionFactory, IUserNotificationService notificationService)
        {
            _connectionFactory = connectionFactory;
            _notificationService = notificationService;
        }

        private async Task EnsureTablesExistAsync()
        {
            using var db = _connectionFactory.CreateConnection();

            var script = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HomeworkProblems' AND xtype='U')
                BEGIN
                    CREATE TABLE HomeworkProblems (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StudentId INT NOT NULL,
                        StudentName NVARCHAR(150) NOT NULL,
                        StudentPhone NVARCHAR(50) NOT NULL,
                        SchoolName NVARCHAR(250) NULL,
                        ClassName NVARCHAR(100) NOT NULL,
                        CurriculumType NVARCHAR(100) NOT NULL,
                        SubjectName NVARCHAR(100) NOT NULL,
                        Title NVARCHAR(250) NOT NULL,
                        Description NVARCHAR(MAX) NOT NULL,
                        ImageUrl NVARCHAR(MAX) NULL,
                        CreatedAt DATETIME DEFAULT GETUTCDATE()
                    );
                END

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProblemReplies' AND xtype='U')
                BEGIN
                    CREATE TABLE ProblemReplies (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        ProblemId INT NOT NULL,
                        UserId INT NOT NULL,
                        UserName NVARCHAR(150) NOT NULL,
                        UserRole NVARCHAR(50) NOT NULL,
                        ReplyText NVARCHAR(MAX) NOT NULL,
                        ImageUrl NVARCHAR(MAX) NULL,
                        CreatedAt DATETIME DEFAULT GETUTCDATE()
                    );
                END

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ReplyLikes' AND xtype='U')
                BEGIN
                    CREATE TABLE ReplyLikes (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        ReplyId INT NOT NULL,
                        UserId INT NOT NULL,
                        CreatedAt DATETIME DEFAULT GETUTCDATE(),
                        CONSTRAINT UQ_Reply_User UNIQUE(ReplyId, UserId)
                    );
                END";

            await db.ExecuteAsync(script);
        }

        public async Task<HomeworkProblem> CreateProblemAsync(
            int studentId,
            string studentName,
            string studentPhone,
            string schoolName,
            string className,
            string curriculumType,
            string subjectName,
            string title,
            string description,
            string? imageUrl)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                INSERT INTO HomeworkProblems (
                    StudentId, StudentName, StudentPhone, SchoolName, 
                    ClassName, CurriculumType, SubjectName, Title, Description, ImageUrl, CreatedAt
                ) 
                VALUES (
                    @StudentId, @StudentName, @StudentPhone, @SchoolName, 
                    @ClassName, @CurriculumType, @SubjectName, @Title, @Description, @ImageUrl, GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var problemId = await db.ExecuteScalarAsync<int>(sql, new
            {
                StudentId = studentId,
                StudentName = studentName,
                StudentPhone = studentPhone,
                SchoolName = schoolName,
                ClassName = className,
                CurriculumType = curriculumType,
                SubjectName = subjectName,
                Title = title,
                Description = description,
                ImageUrl = imageUrl
            });

            // Notify specialist teachers
            var notifyTitle = $"New {subjectName} Homework Problem!";
            var notifyMessage = $"{studentName} ({className}) posted a question: \"{title}\"";
            await _notificationService.NotifySpecialistTeachersAsync(subjectName, notifyTitle, notifyMessage, problemId);

            return new HomeworkProblem
            {
                Id = problemId,
                StudentId = studentId,
                StudentName = studentName,
                StudentPhone = studentPhone,
                SchoolName = schoolName,
                ClassName = className,
                CurriculumType = curriculumType,
                SubjectName = subjectName,
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<HomeworkProblem>> GetTeacherFeedAsync(int teacherId, string? subjectName = null, string? className = null)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT p.*,
                       (SELECT COUNT(1) FROM ProblemReplies r WHERE r.ProblemId = p.Id) AS TotalReplies
                FROM HomeworkProblems p
                WHERE (@SubjectName IS NULL OR p.SubjectName = @SubjectName)
                  AND (@ClassName IS NULL OR p.ClassName = @ClassName)
                ORDER BY p.CreatedAt DESC";

            var problems = await db.QueryAsync<HomeworkProblem>(sql, new { SubjectName = subjectName, ClassName = className });
            return problems.AsList();
        }

        public async Task<List<HomeworkProblem>> GetStudentFeedAsync(int studentId)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT p.*,
                       (SELECT COUNT(1) FROM ProblemReplies r WHERE r.ProblemId = p.Id) AS TotalReplies
                FROM HomeworkProblems p
                WHERE p.StudentId = @StudentId
                ORDER BY p.CreatedAt DESC";

            var problems = await db.QueryAsync<HomeworkProblem>(sql, new { StudentId = studentId });
            return problems.AsList();
        }

        public async Task<HomeworkProblem?> GetProblemByIdAsync(int problemId, int currentUserId)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var problem = await db.QueryFirstOrDefaultAsync<HomeworkProblem>(
                "SELECT * FROM HomeworkProblems WHERE Id = @Id", new { Id = problemId });

            if (problem == null) return null;

            var repliesSql = @"
                SELECT r.*,
                       (SELECT COUNT(1) FROM ReplyLikes l WHERE l.ReplyId = r.Id) AS LikeCount,
                       CASE WHEN EXISTS(SELECT 1 FROM ReplyLikes l WHERE l.ReplyId = r.Id AND l.UserId = @UserId) 
                            THEN 1 ELSE 0 END AS IsLikedByMe
                FROM ProblemReplies r
                WHERE r.ProblemId = @ProblemId
                ORDER BY r.CreatedAt ASC";

            var replies = await db.QueryAsync<ProblemReply>(repliesSql, new { ProblemId = problemId, UserId = currentUserId });
            problem.Replies = replies.AsList();
            problem.TotalReplies = problem.Replies.Count;

            return problem;
        }

        public async Task<ProblemReply> AddReplyAsync(int problemId, int userId, string userName, string userRole, string replyText, string? imageUrl = null, string? audioUrl = null)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                INSERT INTO ProblemReplies (ProblemId, UserId, UserName, UserRole, ReplyText, ImageUrl, AudioUrl, CreatedAt)
                VALUES (@ProblemId, @UserId, @UserName, @UserRole, @ReplyText, @ImageUrl, @AudioUrl, GETUTCDATE());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var replyId = await db.ExecuteScalarAsync<int>(sql, new
            {
                ProblemId = problemId,
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                ReplyText = replyText,
                ImageUrl = imageUrl,
                AudioUrl = audioUrl
            });

            // Notify the student who asked the question
            var problem = await db.QueryFirstOrDefaultAsync<HomeworkProblem>(
                "SELECT StudentId, Title FROM HomeworkProblems WHERE Id = @Id", new { Id = problemId });

            if (problem != null && problem.StudentId != userId)
            {
                var notifyTitle = $"{userRole} {userName} replied to your homework!";
                var notifyMessage = $"Feedback: \"{(replyText.Length > 60 ? replyText.Substring(0, 57) + "..." : replyText)}\"";
                await _notificationService.CreateNotificationAsync(problem.StudentId, notifyTitle, notifyMessage, "ReplyAdded", problemId);
            }

            return new ProblemReply
            {
                Id = replyId,
                ProblemId = problemId,
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                ReplyText = replyText,
                ImageUrl = imageUrl,
                AudioUrl = audioUrl,
                CreatedAt = DateTime.UtcNow,
                LikeCount = 0,
                IsLikedByMe = false
            };
        }

        public async Task<(bool IsLiked, int TotalLikes)> ToggleLikeReplyAsync(int replyId, int userId)
        {
            await EnsureTablesExistAsync();
            using var db = _connectionFactory.CreateConnection();

            var exists = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM ReplyLikes WHERE ReplyId = @ReplyId AND UserId = @UserId",
                new { ReplyId = replyId, UserId = userId });

            bool isLiked;
            if (exists > 0)
            {
                await db.ExecuteAsync(
                    "DELETE FROM ReplyLikes WHERE ReplyId = @ReplyId AND UserId = @UserId",
                    new { ReplyId = replyId, UserId = userId });
                isLiked = false;
            }
            else
            {
                await db.ExecuteAsync(
                    "INSERT INTO ReplyLikes (ReplyId, UserId, CreatedAt) VALUES (@ReplyId, @UserId, GETUTCDATE())",
                    new { ReplyId = replyId, UserId = userId });
                isLiked = true;

                // Notify author of reply
                var reply = await db.QueryFirstOrDefaultAsync<ProblemReply>(
                    "SELECT UserId, ProblemId FROM ProblemReplies WHERE Id = @Id", new { Id = replyId });

                if (reply != null && reply.UserId != userId)
                {
                    await _notificationService.CreateNotificationAsync(
                        reply.UserId, "Your reply received a like!", "A student or teacher liked your solution.", "ReplyLiked", reply.ProblemId);
                }
            }

            var totalLikes = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM ReplyLikes WHERE ReplyId = @ReplyId",
                new { ReplyId = replyId });

            return (isLiked, totalLikes);
        }
    }
}
