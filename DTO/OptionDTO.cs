public class OptionDto
{
    // ✅ Make it nullable — let backend generate if not sent
    public Guid? OptionId { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
