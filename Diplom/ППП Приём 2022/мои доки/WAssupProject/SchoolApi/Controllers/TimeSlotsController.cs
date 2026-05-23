using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/timeslots")]
public class TimeSlotsController : CrudController<TimeSlot>
{
    public TimeSlotsController(AppDbContext context) : base(context) { }
    protected override int GetId(TimeSlot entity) => entity.TimeSlotId;
    protected override void SetId(TimeSlot entity, int id) => entity.TimeSlotId = id;
}
