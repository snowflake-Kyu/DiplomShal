using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/users")]
public class UsersController : CrudController<AppUser>
{
    public UsersController(AppDbContext context) : base(context) { }
    protected override int GetId(AppUser entity) => entity.UserId;
    protected override void SetId(AppUser entity, int id) => entity.UserId = id;
}
