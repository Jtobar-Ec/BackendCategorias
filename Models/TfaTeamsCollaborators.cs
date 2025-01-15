using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoriasAPI.Models
{
    public class TfaTeamsCollaborators
    {
        [Key]
        [Column("colaboratorTeamID")]
        public int CollaboratorTeamId { get; set; }  // Clave primaria compuesta

        [Key]
        [Column("colaboratorUsersID")]
        public int CollaboratorUserId { get; set; }  // Clave primaria compuesta

        // Relación con TfaTeam (Equipo)
        [ForeignKey("CollaboratorTeamId")]
        public virtual TfaTeam Team { get; set; }

        // Relación con TfaUser (Usuario)
        [ForeignKey("CollaboratorUserId")]
        public virtual TfaUser User { get; set; }
    }
}
