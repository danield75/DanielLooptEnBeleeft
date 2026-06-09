using DanielLooptEnBeleeftApi.Application.Services;
using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanielLooptEnBeleeftApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunningRacesController : ControllerBase
{
    private readonly RunningRaceService _service;

    public RunningRacesController(RunningRaceService service)
    {
        _service = service;
    }

    // GET: api/RunningRaces
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RunningRace>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    // GET: api/RunningRaces/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RunningRace>> GetById(int id, CancellationToken ct)
    {
        var race = await _service.GetByIdAsync(id, ct);
        return race is null ? NotFound() : Ok(race);
    }

    // POST: api/RunningRaces
    [HttpPost]
    public async Task<ActionResult<RunningRace>> Create(RunningRace runningRace, CancellationToken ct)
    {
        var created = await _service.CreateAsync(runningRace, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/RunningRaces/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RunningRace runningRace, CancellationToken ct)
    {
        if (id != runningRace.Id) return BadRequest("Id in route does not match body.");

        var ok = await _service.UpdateAsync(id, runningRace, ct);
        return ok ? NoContent() : NotFound();
    }

    // DELETE: api/RunningRaces/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound();
}