namespace Som_Models.DTOs
{
    public class StudentRegisterDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string GuardianPhone { get; set; } = string.Empty;
        public string? GuardianEmail { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string CurriculumType { get; set; } = "General"; // "General" or "English Medium"
        public string ClassName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolAddress { get; set; }
        public string Password { get; set; } = "123";
    }

    public class TeacherRegisterDto
    {
        public string TeacherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolAddress { get; set; }
        public List<string> SpecialistSubjects { get; set; } = new();
        public string Password { get; set; } = "123";
    }

    public class UserLoginDto
    {
        public string Identifier { get; set; } = string.Empty; // Phone or Email
        public string Password { get; set; } = string.Empty;
        public string? DeviceToken { get; set; }
    }

    public class AuthUserResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Student", "Teacher", "Admin"
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? SchoolName { get; set; }
        public string? ClassName { get; set; }
        public string? CurriculumType { get; set; }
        public List<string>? SpecialistSubjects { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class CreateTuitionPostDto
    {
        public string CurriculumType { get; set; } = "General";
        public string ClassName { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Salary { get; set; }
        public string? DaysPerWeek { get; set; }
        public string TuitionType { get; set; } = "Home Tuition";
        public string? Description { get; set; }
    }

    public class ApplyTuitionDto
    {
        public string? Qualifications { get; set; }
        public string? Experience { get; set; }
        public string? Message { get; set; }
    }
}
