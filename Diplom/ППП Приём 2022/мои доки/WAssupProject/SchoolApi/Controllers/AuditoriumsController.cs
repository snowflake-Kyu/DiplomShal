using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/auditoriums")]
public class AuditoriumsController : CrudController<Auditorium>
{
    public AuditoriumsController(AppDbContext context) : base(context) { }
    protected override int GetId(Auditorium entity) => entity.AuditoriumId;
    protected override void SetId(Auditorium entity, int id) => entity.AuditoriumId = id;
}
