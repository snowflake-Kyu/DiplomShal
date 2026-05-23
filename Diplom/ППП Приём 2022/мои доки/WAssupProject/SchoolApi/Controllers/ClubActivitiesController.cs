using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/clubactivities")]
public class ClubActivitiesController : CrudController<ClubActivity>
{
    public ClubActivitiesController(AppDbContext context) : base(context) { }
    protected override int GetId(ClubActivity entity) => entity.ClubActivityId;
    protected override void SetId(ClubActivity entity, int id) => entity.ClubActivityId = id;
}
