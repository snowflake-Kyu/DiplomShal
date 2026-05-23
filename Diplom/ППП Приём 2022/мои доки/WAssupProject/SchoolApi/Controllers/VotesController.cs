using Microsoft.AspNetCore.Mvc;
using SchoolApi.Data;
using SchoolApi.Models;

namespace SchoolApi.Controllers;

[Route("api/votes")]
public class VotesController : CrudController<Vote>
{
    public VotesController(AppDbContext context) : base(context) { }
    protected override int GetId(Vote entity) => entity.VoteId;
    protected override void SetId(Vote entity, int id) => entity.VoteId = id;
}
