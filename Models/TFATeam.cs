using System;
using System.Collections.Generic;

namespace CategoriasAPI.Models;

public partial class TfaTeam
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = null!;

    public string TeamDescription { get; set; } = null!;

    public int TeamStatusId { get; set; }

    public int? TeamLeadId { get; set; }

    public int? CategoriesId { get; set; }

    public virtual TfaCategory? Categories { get; set; }

    public virtual TfaUser? TeamLead { get; set; }

    public virtual TfaTeamstatus TeamStatus { get; set; } = null!;

    // Relación con la tabla intermedia
    public virtual ICollection<TfaTeamsCategories> TfaTeamsCategories { get; set; } = new List<TfaTeamsCategories>();

    // Relación con los colaboradores
    public virtual ICollection<TfaUser> ColaboratorUsers { get; set; } = new List<TfaUser>();
}
