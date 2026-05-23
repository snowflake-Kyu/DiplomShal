using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/teachingassignments")]
public class TeachingAssignmentsController : CrudController<TeachingAssignment>
{
    public TeachingAssignmentsController(AppDbContext context) : base(context) { }
    protected override int GetId(TeachingAssignment entity) => entity.AssignmentId;
    protected override void SetId(TeachingAssignment entity, int id) => entity.AssignmentId = id;
}
