using Dapper;
using Som_Models.DTOs;
using Som_Models.Models;
using Som_Service.Interface;

namespace Som_Service.Service
{
    public class TuitionService : ITuitionService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IUserNotificationService _notificationService;

        public TuitionService(IDbConnectionFactory connectionFactory, IUserNotificationService notificationService)
        {
            _connectionFactory = connectionFactory;
            _notificationService = notificationService;
        }

        public async Task<TuitionPost> CreateTuitionPostAsync(int studentOrGuardianId, string posterName, string phone, CreateTuitionPostDto dto)
        {
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                INSERT INTO TuitionPosts (
                    StudentOrGuardianId, PosterName, Phone, CurriculumType,
                    ClassName, Subjects, Location, Salary, DaysPerWeek,
                    TuitionType, Description, Status, CreatedAt
                )
                VALUES (
                    @StudentOrGuardianId, @PosterName, @Phone, @CurriculumType,
                    @ClassName, @Subjects, @Location, @Salary, @DaysPerWeek,
                    @TuitionType, @Description, 'Open', GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await db.ExecuteScalarAsync<int>(sql, new
            {
                StudentOrGuardianId = studentOrGuardianId,
                PosterName = posterName,
                Phone = phone,
                CurriculumType = dto.CurriculumType,
                ClassName = dto.ClassName,
                Subjects = dto.Subjects,
                Location = dto.Location,
                Salary = dto.Salary,
                DaysPerWeek = dto.DaysPerWeek,
                TuitionType = dto.TuitionType,
                Description = dto.Description
            });

            return new TuitionPost
            {
                Id = id,
                StudentOrGuardianId = studentOrGuardianId,
                PosterName = posterName,
                Phone = phone,
                CurriculumType = dto.CurriculumType,
                ClassName = dto.ClassName,
                Subjects = dto.Subjects,
                Location = dto.Location,
                Salary = dto.Salary,
                DaysPerWeek = dto.DaysPerWeek,
                TuitionType = dto.TuitionType,
                Description = dto.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                TotalApplicants = 0,
                HasApplied = false
            };
        }

        public async Task<List<TuitionPost>> GetTuitionPostsAsync(int currentUserId, string? className = null, string? location = null, string? curriculum = null)
        {
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT p.*,
                       (SELECT COUNT(1) FROM TuitionApplications a WHERE a.TuitionPostId = p.Id) AS TotalApplicants,
                       CASE WHEN EXISTS(SELECT 1 FROM TuitionApplications a WHERE a.TuitionPostId = p.Id AND a.TeacherId = @UserId)
                            THEN 1 ELSE 0 END AS HasApplied
                FROM TuitionPosts p
                WHERE p.Status = 'Open'
                  AND (@ClassName IS NULL OR p.ClassName = @ClassName)
                  AND (@Location IS NULL OR p.Location LIKE '%' + @Location + '%')
                  AND (@Curriculum IS NULL OR p.CurriculumType = @Curriculum)
                ORDER BY p.CreatedAt DESC";

            var posts = await db.QueryAsync<TuitionPost>(sql, new
            {
                UserId = currentUserId,
                ClassName = className,
                Location = location,
                Curriculum = curriculum
            });

            return posts.AsList();
        }

        public async Task<TuitionApplication> ApplyForTuitionAsync(int tuitionPostId, int teacherId, string teacherName, string teacherPhone, ApplyTuitionDto dto)
        {
            using var db = _connectionFactory.CreateConnection();

            var existing = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM TuitionApplications WHERE TuitionPostId = @PostId AND TeacherId = @TeacherId",
                new { PostId = tuitionPostId, TeacherId = teacherId });

            if (existing > 0)
            {
                throw new ArgumentException("You have already applied for this tuition requirement.");
            }

            var sql = @"
                INSERT INTO TuitionApplications (
                    TuitionPostId, TeacherId, TeacherName, TeacherPhone,
                    Qualifications, Experience, Message, Status, CreatedAt
                )
                VALUES (
                    @TuitionPostId, @TeacherId, @TeacherName, @TeacherPhone,
                    @Qualifications, @Experience, @Message, 'Pending', GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var appId = await db.ExecuteScalarAsync<int>(sql, new
            {
                TuitionPostId = tuitionPostId,
                TeacherId = teacherId,
                TeacherName = teacherName,
                TeacherPhone = teacherPhone,
                Qualifications = dto.Qualifications,
                Experience = dto.Experience,
                Message = dto.Message
            });

            // Notify guardian / student who posted
            var post = await db.QueryFirstOrDefaultAsync<TuitionPost>(
                "SELECT StudentOrGuardianId, Subjects, ClassName FROM TuitionPosts WHERE Id = @Id",
                new { Id = tuitionPostId });

            if (post != null)
            {
                var title = "New Teacher Application for your Tuition!";
                var msg = $"{teacherName} applied for your {post.ClassName} ({post.Subjects}) tuition requirement.";
                await _notificationService.CreateNotificationAsync(post.StudentOrGuardianId, title, msg, "TuitionApplication", tuitionPostId);
            }

            return new TuitionApplication
            {
                Id = appId,
                TuitionPostId = tuitionPostId,
                TeacherId = teacherId,
                TeacherName = teacherName,
                TeacherPhone = teacherPhone,
                Qualifications = dto.Qualifications,
                Experience = dto.Experience,
                Message = dto.Message,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<TuitionApplication>> GetPostApplicationsAsync(int tuitionPostId, int currentUserId)
        {
            using var db = _connectionFactory.CreateConnection();

            // Verify owner or teacher
            var sql = @"
                SELECT a.* FROM TuitionApplications a
                INNER JOIN TuitionPosts p ON p.Id = a.TuitionPostId
                WHERE a.TuitionPostId = @PostId
                  AND (p.StudentOrGuardianId = @UserId OR a.TeacherId = @UserId)
                ORDER BY a.CreatedAt DESC";

            var list = await db.QueryAsync<TuitionApplication>(sql, new { PostId = tuitionPostId, UserId = currentUserId });
            return list.AsList();
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, int currentUserId, string status)
        {
            using var db = _connectionFactory.CreateConnection();

            var sql = @"
                UPDATE a
                SET a.Status = @Status
                FROM TuitionApplications a
                INNER JOIN TuitionPosts p ON p.Id = a.TuitionPostId
                WHERE a.Id = @AppId AND p.StudentOrGuardianId = @UserId";

            var affected = await db.ExecuteAsync(sql, new { AppId = applicationId, UserId = currentUserId, Status = status });
            return affected > 0;
        }
    }
}
