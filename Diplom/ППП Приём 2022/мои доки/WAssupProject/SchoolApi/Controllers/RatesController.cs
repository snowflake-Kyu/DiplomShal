using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/rates")]
public class RatesController : CrudController<Rate>
{
    public RatesController(AppDbContext context) : base(context) { }
    protected override int GetId(Rate entity) => entity.RateId;
    protected override void SetId(Rate entity, int id) => entity.RateId = id;
}
