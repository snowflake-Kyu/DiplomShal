using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/disciplines")]
public class DisciplinesController : CrudController<Discipline>
{
    public DisciplinesController(AppDbContext context) : base(context) { }
    protected override int GetId(Discipline entity) => entity.DisciplineId;
    protected override void SetId(Discipline entity, int id) => entity.DisciplineId = id;
}
