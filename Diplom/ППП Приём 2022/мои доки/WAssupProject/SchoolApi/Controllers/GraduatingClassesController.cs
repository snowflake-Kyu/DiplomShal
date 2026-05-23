using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/graduatingclasses")]
public class GraduatingClassesController : CrudController<GraduatingClass>
{
    public GraduatingClassesController(AppDbContext context) : base(context) { }
    protected override int GetId(GraduatingClass entity) => entity.GraduatingClassId;
    protected override void SetId(GraduatingClass entity, int id) => entity.GraduatingClassId = id;
}
