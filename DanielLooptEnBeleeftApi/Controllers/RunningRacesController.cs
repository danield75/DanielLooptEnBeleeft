using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanielLooptEnBeleeftApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunningRacesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RunningRacesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/RunningRaces
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RunningRace>>> GetAll()
    {
        var races = await _context.RunningRaces
            .Include(r => r.RaceReport)
            .OrderByDescending(r => r.Datum)
            .ToListAsync();

        return Ok(races);
    }

    // GET: api/RunningRaces/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RunningRace>> GetById(int id)
    {
        var race = await _context.RunningRaces
            .Include(r => r.RaceReport)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (race == null)
        {
            return NotFound();
        }

        return Ok(race);
    }

    // POST: api/RunningRaces
    [HttpPost]
    public async Task<ActionResult<RunningRace>> Create(RunningRace runningRace)
    {
        _context.RunningRaces.Add(runningRace);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = runningRace.Id }, runningRace);
    }

    // PUT: api/RunningRaces/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RunningRace runningRace)
    {
        if (id != runningRace.Id)
        {
            return BadRequest("Id in route does not match body.");
        }

        var existingRace = await _context.RunningRaces
            .Include(r => r.RaceReport)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRace == null)
        {
            return NotFound();
        }

        existingRace.Datum = runningRace.Datum;
        existingRace.Distance = runningRace.Distance;
        existingRace.DistanceUnit = runningRace.DistanceUnit;
        existingRace.RaceName = runningRace.RaceName;
        existingRace.RacePlace = runningRace.RacePlace;
        existingRace.RaceStartTime = runningRace.RaceStartTime;
        existingRace.LinkToRaceWebsite = runningRace.LinkToRaceWebsite;
        existingRace.FinishTime = runningRace.FinishTime;
        existingRace.OverallPlace = runningRace.OverallPlace;
        existingRace.NumberParticipantsOverall = runningRace.NumberParticipantsOverall;
        existingRace.AgeCategoryPlace = runningRace.AgeCategoryPlace;
        existingRace.NumberParticipantsAgeCategory = runningRace.NumberParticipantsAgeCategory;
        existingRace.LinkToRaceResult = runningRace.LinkToRaceResult;
        existingRace.LinkToRaceReport = runningRace.LinkToRaceReport;

        if (runningRace.RaceReport is null)
        {
            existingRace.RaceReport = null;
        }
        else if (existingRace.RaceReport is null)
        {
            existingRace.RaceReport = new RaceReport
            {
                RunningRaceId = existingRace.Id,
                ReportText = runningRace.RaceReport.ReportText
            };
        }
        else
        {
            existingRace.RaceReport.ReportText = runningRace.RaceReport.ReportText;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/RunningRaces/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var race = await _context.RunningRaces
            .Include(r => r.RaceReport)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (race == null)
        {
            return NotFound();
        }

        _context.RunningRaces.Remove(race);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}