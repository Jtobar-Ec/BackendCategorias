using System;
using System.Collections.Generic;

namespace CategoriasAPI.Models;

public partial class TfaCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? CategoryDescription { get; set; }

    public int? CategoryPoints { get; set; }

    public DateOnly? CategoryDeadLine { get; set; }

    public int? ReducePoints { get; set; }

    public virtual ICollection<TfaTeamsCategories> TfaTeamsCategories { get; set; } = new List<TfaTeamsCategories>();

    public virtual ICollection<TfaCertificate> TfaCertificates { get; set; } = new List<TfaCertificate>();

    public virtual ICollection<TfaHistory> TfaHistories { get; set; } = new List<TfaHistory>();

    public virtual ICollection<TfaTask> TfaTasks { get; set; } = new List<TfaTask>();

    public virtual ICollection<TfaTeam> TfaTeams { get; set; } = new List<TfaTeam>();
}
