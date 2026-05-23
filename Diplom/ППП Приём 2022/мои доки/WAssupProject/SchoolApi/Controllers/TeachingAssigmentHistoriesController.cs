using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/teachingassigmenthistories")]
public class TeachingAssigmentHistoriesController : CrudController<TeachingAssigmentHistory>
{
    public TeachingAssigmentHistoriesController(AppDbContext context) : base(context) { }
    protected override int GetId(TeachingAssigmentHistory entity) => entity.TeachingAssigmentHistoryId;
    protected override void SetId(TeachingAssigmentHistory entity, int id) => entity.TeachingAssigmentHistoryId = id;
}
