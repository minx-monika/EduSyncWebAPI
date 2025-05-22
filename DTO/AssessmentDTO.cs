namespace EduSyncWebAPI.DTOs
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public int? MaxScore { get; set; }
        public Guid? CourseId { get; set; }
    }

    public class AssessmentSummaryDTO
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public int? MaxScore { get; set; }
    }

    public class AssessmentDetailDto
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public int? MaxScore { get; set; }

        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

        public CourseReadDTO? Course { get; set; }
    }

    // New DTO for create/update that includes Questions
    public class AssessmentCreateUpdateDto
    {
        // ✅ Make it nullable so it's optional during creation
        public Guid? AssessmentId { get; set; }

        public string? Title { get; set; }
        public int? MaxScore { get; set; }
        public Guid? CourseId { get; set; }

        // ✅ Keep it nullable or empty list for safety
        public List<QuestionDto>? Questions { get; set; } = new List<QuestionDto>();
    }


    public class AssessmentSubmissionDto
    {
        public Guid AssessmentId { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }

    public class AnswerDto
    {
        public Guid QuestionId { get; set; }
        public Guid SelectedOptionId { get; set; }
    }
}
