using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/schedules")]
public class SchedulesController : CrudController<Schedule>
{
    public SchedulesController(AppDbContext context) : base(context) { }
    protected override int GetId(Schedule entity) => entity.ScheduleId;
    protected override void SetId(Schedule entity, int id) => entity.ScheduleId = id;
}
