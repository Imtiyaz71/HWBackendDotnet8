namespace Som_Models.Models
{
    public class HomeworkProblem
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CurriculumType { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalReplies { get; set; }
        public int TotalLikes { get; set; }
        public bool IsLikedByMe { get; set; }
        public List<ProblemReply> Replies { get; set; } = new();
    }

    public class ProblemReply
    {
        public int Id { get; set; }
        public int ProblemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty; // "Teacher", "Student", "AI"
        public string ReplyText { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int LikeCount { get; set; }
        public bool IsLikedByMe { get; set; }
    }

    public class ReplyLike
    {
        public int Id { get; set; }
        public int ReplyId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "ProblemCreated", "ReplyAdded", "ReplyLiked"
        public int? RelatedProblemId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TuitionPost
    {
        public int Id { get; set; }
        public int StudentOrGuardianId { get; set; }
        public string PosterName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CurriculumType { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Salary { get; set; }
        public string? DaysPerWeek { get; set; }
        public string TuitionType { get; set; } = "Home Tuition";
        public string? Description { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalApplicants { get; set; }
        public bool HasApplied { get; set; }
    }

    public class TuitionApplication
    {
        public int Id { get; set; }
        public int TuitionPostId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherPhone { get; set; } = string.Empty;
        public string? Qualifications { get; set; }
        public string? Experience { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
