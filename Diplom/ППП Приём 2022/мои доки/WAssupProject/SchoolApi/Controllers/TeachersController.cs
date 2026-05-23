using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/teachers")]
public class TeachersController : CrudController<Teacher>
{
    public TeachersController(AppDbContext context) : base(context) { }
    protected override int GetId(Teacher entity) => entity.TeacherId;
    protected override void SetId(Teacher entity, int id) => entity.TeacherId = id;
}
