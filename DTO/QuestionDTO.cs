public class QuestionDto
{
    // ✅ Make it nullable so it's not required during POST
    public Guid? QuestionId { get; set; }

    public string Text { get; set; } = string.Empty;
    public int Marks { get; set; }

    public List<OptionDto> Options { get; set; } = new List<OptionDto>();
}
