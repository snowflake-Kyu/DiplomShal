using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolApi.Data;

namespace SchoolApi.Controllers;

[ApiController]
public abstract class CrudController<TEntity> : ControllerBase where TEntity : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected CrudController(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    protected abstract int GetId(TEntity entity);
    protected abstract void SetId(TEntity entity, int id);

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        var items = await DbSet.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TEntity>> GetById(int id)
    {
        var item = await DbSet.FindAsync(id);

        if (item == null)
        {
            return NotFound(new { Message = $"Запись с id={id} не найдена." });
        }

        return Ok(item);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        DbSet.Add(entity);

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                Message = "Не удалось добавить запись. Проверьте внешние ключи, обязательные поля и ограничения базы данных.",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = GetId(entity) }, entity);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, [FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await DbSet.FindAsync(id);

        if (existing == null)
        {
            return NotFound(new { Message = $"Запись с id={id} не найдена." });
        }

        SetId(entity, id);
        Context.Entry(existing).CurrentValues.SetValues(entity);

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                Message = "Не удалось обновить запись. Проверьте внешние ключи, обязательные поля и ограничения базы данных.",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var existing = await DbSet.FindAsync(id);

        if (existing == null)
        {
            return NotFound(new { Message = $"Запись с id={id} не найдена." });
        }

        DbSet.Remove(existing);

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                Message = "Не удалось удалить запись. Возможно, на неё ссылаются другие таблицы.",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }

        return NoContent();
    }
}
