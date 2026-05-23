using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/academicyears")]
public class AcademicYearsController : CrudController<AcademicYear>
{
    public AcademicYearsController(AppDbContext context) : base(context) { }
    protected override int GetId(AcademicYear entity) => entity.AcademicYearId;
    protected override void SetId(AcademicYear entity, int id) => entity.AcademicYearId = id;
}
