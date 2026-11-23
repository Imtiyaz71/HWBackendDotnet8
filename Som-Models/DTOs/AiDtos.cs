namespace Som_Models.DTOs
{
    public class AiSolveRequest
    {
        public int? ProblemId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string? ImageBase64 { get; set; }
        public string? UserApiKey { get; set; }
    }

    public class AiSolveResponse
    {
        public bool Success { get; set; } = true;
        public string Solution { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
