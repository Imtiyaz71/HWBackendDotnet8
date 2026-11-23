using Som_Models.DTOs;
using Som_Models.Models;

namespace Som_Service.Interface
{
    public interface IOpenAuthService
    {
        Task<AuthUserResponse> RegisterStudentAsync(StudentRegisterDto dto);
        Task<AuthUserResponse> RegisterTeacherAsync(TeacherRegisterDto dto);
        Task<AuthUserResponse> LoginUserAsync(UserLoginDto dto);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }

    public interface IAcademicService
    {
        Task<List<Curriculum>> GetCurriculumsAsync();
        Task<List<string>> GetClassesAsync(string curriculumType);
        Task<List<string>> GetSubjectsAsync(string curriculumType, string className);
        Task SeedAcademicDataAsync();
    }

    public interface IProblemFeedService
    {
        Task<HomeworkProblem> CreateProblemAsync(
            int studentId,
            string studentName,
            string studentPhone,
            string schoolName,
            string className,
            string curriculumType,
            string subjectName,
            string title,
            string description,
            string? imageUrl);

        Task<List<HomeworkProblem>> GetTeacherFeedAsync(int teacherId, string? subjectName = null, string? className = null);
        Task<List<HomeworkProblem>> GetStudentFeedAsync(int studentId);
        Task<HomeworkProblem?> GetProblemByIdAsync(int problemId, int currentUserId);
        Task<ProblemReply> AddReplyAsync(int problemId, int userId, string userName, string userRole, string replyText, string? imageUrl = null, string? audioUrl = null);
        Task<(bool IsLiked, int TotalLikes)> ToggleLikeReplyAsync(int replyId, int userId);
    }

    public interface ITuitionService
    {
        Task<TuitionPost> CreateTuitionPostAsync(int studentOrGuardianId, string posterName, string phone, CreateTuitionPostDto dto);
        Task<List<TuitionPost>> GetTuitionPostsAsync(int currentUserId, string? className = null, string? location = null, string? curriculum = null);
        Task<TuitionApplication> ApplyForTuitionAsync(int tuitionPostId, int teacherId, string teacherName, string teacherPhone, ApplyTuitionDto dto);
        Task<List<TuitionApplication>> GetPostApplicationsAsync(int tuitionPostId, int currentUserId);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, int currentUserId, string status);
    }

    public interface IAiAssistantService
    {
        Task<AiSolveResponse> SolveHomeworkWithAiAsync(AiSolveRequest request);
    }

    public interface IUserNotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, string type, int? relatedProblemId = null);
        Task<List<UserNotification>> GetNotificationsAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task NotifySpecialistTeachersAsync(string subjectName, string title, string message, int problemId);
    }
}
