using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/educationalprograms")]
public class EducationalProgramsController : CrudController<EducationalProgram>
{
    public EducationalProgramsController(AppDbContext context) : base(context) { }
    protected override int GetId(EducationalProgram entity) => entity.ProgramId;
    protected override void SetId(EducationalProgram entity, int id) => entity.ProgramId = id;
}
