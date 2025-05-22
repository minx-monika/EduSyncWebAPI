using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSyncWebAPI.Models
{
    public class Question
    {
        public Guid QuestionId { get; set; }

        public Guid AssessmentId { get; set; }

        public string Text { get; set; } = string.Empty;

        public int Marks { get; set; }

        // Navigation properties
        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; } = null!;

        public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    }
}
