
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;

namespace Studying_With_Future.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseController(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<T>> GetById(int id)
        {
            var entidade = await _dbSet.FindAsync(id);
            if (entidade == null)
            {
                return NotFound();
            }

            return entidade;
        }

        [HttpPost]
        public virtual async Task<ActionResult<T>> Create(T entidade)
        {
            _dbSet.Add(entidade);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = GetEntityId(entidade) }, entidade);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Update(int id, T entidade)
        {
            if (id != GetEntityId(entidade))
            {
                return BadRequest();
            }

            _context.Entry(entidade).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var entidade = await _dbSet.FindAsync(id);
            if (entidade == null)
            {
                return NotFound();
            }

            _dbSet.Remove(entidade);
            await _context.SaveChangesAsync();
            return NoContent();
        }

         private int GetEntityId(T entity)
        {
            var property = typeof(T).GetProperty("Id");
            return (int)property.GetValue(entity);
        }
    }
}