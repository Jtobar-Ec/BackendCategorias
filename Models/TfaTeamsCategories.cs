using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoriasAPI.Models
{
    public class TfaTeamsCategories
    {
        [ForeignKey("Team")]
        public int TeamId { get; set; }

        [ForeignKey("Category")]
        public int CategoriesId { get; set; }

        // Propiedades de navegación
        public TfaTeam? Team { get; set; }
        public TfaCategory? Category { get; set; }
    }
}
