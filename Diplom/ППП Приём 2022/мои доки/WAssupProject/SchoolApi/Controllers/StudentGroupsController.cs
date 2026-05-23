using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/studentgroups")]
public class StudentGroupsController : CrudController<StudentGroup>
{
    public StudentGroupsController(AppDbContext context) : base(context) { }
    protected override int GetId(StudentGroup entity) => entity.GroupId;
    protected override void SetId(StudentGroup entity, int id) => entity.GroupId = id;
}
