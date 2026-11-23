namespace Som_Models.Models
{
    public class Curriculum
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "General", "English Medium"
        public string Description { get; set; } = string.Empty;
    }

    public class CurriculumSubject
    {
        public int Id { get; set; }
        public string CurriculumType { get; set; } = string.Empty; // "General", "English Medium"
        public string ClassName { get; set; } = string.Empty;      // "Class 1" ... "Class 10" / "Grade 1" ... "Grade 12"
        public string SubjectName { get; set; } = string.Empty;    // "Mathematics", "Bangla", "Physics", etc.
    }
}
