using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/schoolclasses")]
public class SchoolClassesController : CrudController<SchoolClass>
{
    public SchoolClassesController(AppDbContext context) : base(context) { }
    protected override int GetId(SchoolClass entity) => entity.ClassId;
    protected override void SetId(SchoolClass entity, int id) => entity.ClassId = id;
}
