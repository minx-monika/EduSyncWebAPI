using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSyncWebAPI.Models
{
    public class Option
    {
        public Guid OptionId { get; set; }

        public Guid QuestionId { get; set; }

        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        // Navigation property
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
    }
}
