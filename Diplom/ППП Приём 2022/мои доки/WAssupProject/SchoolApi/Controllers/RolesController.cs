using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/roles")]
public class RolesController : CrudController<Role>
{
    public RolesController(AppDbContext context) : base(context) { }
    protected override int GetId(Role entity) => entity.RoleId;
    protected override void SetId(Role entity, int id) => entity.RoleId = id;
}
