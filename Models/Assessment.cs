using System;
using System.Collections.Generic;

namespace EduSyncWebAPI.Models;

public partial class Assessment
{
    public Guid AssessmentId { get; set; }

    // If course is mandatory, make this non-nullable
    public Guid CourseId { get; set; }

    public string? Title { get; set; }
    public int? MaxScore { get; set; }

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
