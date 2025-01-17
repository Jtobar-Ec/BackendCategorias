using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CategoriasAPI.Data;
using CategoriasAPI.Models;

namespace CategoriasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TfaCategory>>> GetTfaCategories()
        {
            return await _context.TfaCategories.ToListAsync();
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TfaCategory>> GetTfaCategory(int id)
        {
            var tfaCategory = await _context.TfaCategories.FindAsync(id);

            if (tfaCategory == null)
            {
                return NotFound();
            }

            return tfaCategory;
        }

        // PUT: api/Categorias/
        [HttpPut("actualizarCategoria/{id}")]
        public async Task<IActionResult> PutTfaCategory(int id,
        string categoryName,
        string? categoryDescription,
        int categoryPoints,
        DateTime categoryDeadLine,
        int teamId)
        {
            // Validar que los puntos no sean negativos
            if (categoryPoints < 0)
            {
                return BadRequest("Los puntos de la categoría no pueden ser negativos.");
            }

            // Buscar la categoría a actualizar
            var tfaCategory = await _context.TfaCategories.FindAsync(id);
            if (tfaCategory == null)
            {
                return NotFound("La categoría con el ID proporcionado no existe.");
            }

            // Buscar el equipo en la base de datos
            var team = await _context.TfaTeams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("El equipo con el ID proporcionado no existe.");
            }

            // Actualizar los campos de la categoría
            tfaCategory.CategoryName = categoryName;
            tfaCategory.CategoryDescription = categoryDescription;
            tfaCategory.CategoryPoints = categoryPoints;
            tfaCategory.CategoryDeadLine = DateOnly.FromDateTime(categoryDeadLine); // Conversión explícita

            // Verificar si la fecha límite ha pasado y reducir los puntos si es necesario
            if (categoryDeadLine < DateTime.Today)
            {
                tfaCategory.CategoryPoints -= 10; // Reducción de puntos
                if (tfaCategory.CategoryPoints < 0)
                {
                    tfaCategory.CategoryPoints = 0; // No permitir puntos negativos
                }
            }

            // Marcar la entidad como modificada
            _context.Entry(tfaCategory).State = EntityState.Modified;

            try
            {
                // Guardar los cambios en la categoría
                await _context.SaveChangesAsync();

                // Manejar la relación en la tabla intermedia
                var existingTeamCategory = await _context.TfaTeamsCategories
                    .FirstOrDefaultAsync(tc => tc.CategoriesId == tfaCategory.CategoryId);

                if (existingTeamCategory != null)
                {
                    // Eliminar la relación existente
                    _context.TfaTeamsCategories.Remove(existingTeamCategory);
                    await _context.SaveChangesAsync();
                }

                // Crear una nueva relación con el equipo actualizado
                var newTeamCategory = new TfaTeamsCategories
                {
                    TeamId = team.TeamId,
                    CategoriesId = tfaCategory.CategoryId
                };

                _context.TfaTeamsCategories.Add(newTeamCategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TfaCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<object>> PostTfaCategory(
            string categoryName,
            string? categoryDescription,
            int categoryPoints,
            DateTime categoryDeadLine,
            int teamId)
        {
            // Validar que los puntos no sean negativos
            if (categoryPoints < 0)
            {
                return BadRequest("Los puntos de la categoría no pueden ser negativos.");
            }

            // Buscar el equipo en la base de datos
            var team = await _context.TfaTeams.FindAsync(teamId);
            if (team == null)
            {
                return NotFound("El equipo con el ID proporcionado no existe.");
            }

            // Crear la nueva categoría
            var tfaCategory = new TfaCategory
            {
                CategoryName = categoryName,
                CategoryDescription = categoryDescription,
                CategoryPoints = categoryPoints,
                CategoryDeadLine = DateOnly.FromDateTime(categoryDeadLine) // Conversión explícita
            };

            // Verificar si la fecha límite ha pasado y reducir los puntos si es necesario
            if (categoryDeadLine < DateTime.Today)
            {
                tfaCategory.CategoryPoints -= 10; // Reducción de puntos
                if (tfaCategory.CategoryPoints < 0)
                {
                    tfaCategory.CategoryPoints = 0; // No permitir puntos negativos
                }
            }

            // Agregar la categoría al contexto (se guarda primero)
            _context.TfaCategories.Add(tfaCategory);

            try
            {
                // Guardar los cambios en la base de datos (esto asignará el CategoryId)
                await _context.SaveChangesAsync();

                // Ahora que la categoría tiene un CategoryId válido, podemos agregarla a la tabla intermedia
                var teamCategory = new TfaTeamsCategories
                {
                    TeamId = team.TeamId,
                    CategoriesId = tfaCategory.CategoryId // Ahora tiene un valor válido
                };
                _context.TfaTeamsCategories.Add(teamCategory);

                // Guardar la relación en la tabla intermedia
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Hubo un error al guardar la categoría: {ex.Message}");
            }

            // Devolver la respuesta con la categoría creada
            return CreatedAtAction("GetTfaCategory", new { id = tfaCategory.CategoryId }, new
            {
                tfaCategory.CategoryId,
                tfaCategory.CategoryName,
                tfaCategory.CategoryDescription,
                tfaCategory.CategoryPoints,
                tfaCategory.CategoryDeadLine
            });
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTfaCategory(int id)
        {
            var tfaCategory = await _context.TfaCategories.FindAsync(id);
            if (tfaCategory == null)
            {
                return NotFound();
            }

            // Iniciar una transacción
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Eliminar las relaciones en la tabla intermedia TfaTeamsCategories
                    var teamCategories = _context.TfaTeamsCategories.Where(tc => tc.CategoriesId == id);
                    if (teamCategories.Any())
                    {
                        _context.TfaTeamsCategories.RemoveRange(teamCategories);
                        await _context.SaveChangesAsync();  // Guardar cambios en la tabla intermedia
                    }

                    // Ahora eliminar la categoría
                    _context.TfaCategories.Remove(tfaCategory);
                    await _context.SaveChangesAsync(); // Eliminar la categoría

                    // Confirmar la transacción
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    // Si ocurre algún error, revertir la transacción
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return NoContent();
        }




        private bool TfaCategoryExists(int id)
        {
            return _context.TfaCategories.Any(e => e.CategoryId == id);
        }
    }
}
