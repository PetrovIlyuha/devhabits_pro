using System.Text.Json;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;
[Route("habits")]
[ApiController]
public class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        List<HabitDto> habits = await dbContext.Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();
        var habitsCollectionDto = new HabitsCollectionDto
        {
            Items = habits,
        };
        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        HabitWithTagsDto habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToWithTagsDto())
            .FirstOrDefaultAsync();
        if (habit is null)
        {
            return NotFound();
        }
        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(CreateHabitDto createHabitDto)
    {
        Habit habit = createHabitDto.ToEntity();
        await dbContext.Habits.AddAsync(habit);
        await dbContext.SaveChangesAsync();
        HabitDto habitDto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HabitDto>> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit habit = await dbContext
            .Habits
            .FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        
        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }


    [HttpPatch("{id}")]
    public async Task<ActionResult> PartialUpdateHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit habit = await dbContext
            .Habits
            .FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto) || !ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return StatusCode(StatusCodes.Status410Gone, "Habit was removed from the system.");
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
 
    //[HttpPatch("/partial_update/{id}")]
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S6580:Use a format provider when parsing date and time", Justification = "<Pending>")]
    //public async Task<IActionResult> TargetedPartialUpdate(string id, [FromBody] JsonElement patchData)
    //{
    //    Habit habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    // Loop through each property in the JSON body
    //    foreach (JsonProperty prop in patchData.EnumerateObject())
    //    {
    //        switch (prop.Name.ToLower())
    //        {
    //            case "name":
    //                habit.Name = prop.Value.GetString() ?? "";
    //                break;

    //            case "description":
    //                habit.Description = prop.Value.GetString();
    //                break;

    //            case "type":
    //                habit.Type = Enum.TryParse<HabitType>(prop.Value.GetString(), true, out var parsedType)
    //                    ? parsedType
    //                    : habit.Type;
    //                break;

    //            case "status":
    //                habit.Status = Enum.TryParse<HabitStatus>(prop.Value.GetString(), true, out HabitStatus parsedStatus)
    //                    ? parsedStatus
    //                    : habit.Status;
    //                break;

    //            case "is_archived":
    //                habit.IsArchived = prop.Value.GetBoolean();
    //                break;

    //            case "end_date":
    //                if (DateTime.TryParse(prop.Value.GetString(), out DateOnly endDate))
    //                {
    //                    habit.EndDate = endDate;
    //                }
    //                break;

    //            case "frequency":
    //                if (prop.Value.TryGetProperty("type", out FrequencyType freqType))
    //                {
    //                    habit.Frequency.Type = freqType.GetString();
    //                }
    //                if (prop.Value.TryGetProperty("times_per_period", out var timesPerPeriod))
    //                {
    //                    habit.Frequency.TimesPerPeriod = timesPerPeriod.GetInt32();
    //                }
    //                break;

    //            case "target":
    //                if (prop.Value.TryGetProperty("value", out var targetValue))
    //                {
    //                    habit.Target.Value = targetValue.GetInt32();
    //                }
    //                if (prop.Value.TryGetProperty("unit", out var targetUnit))
    //                {
    //                    habit.Target.Unit = targetUnit.GetString();
    //                }
    //                break;

    //            case "milestone":
    //                if (prop.Value.TryGetProperty("target", out var milestoneTarget))
    //                {
    //                    habit.Milestone.Target = milestoneTarget.GetInt32();
    //                }
    //                if (prop.Value.TryGetProperty("current", out var milestoneCurrent))
    //                {
    //                    habit.Milestone.Current = milestoneCurrent.GetInt32();
    //                }
    //                break;

    //            default:
    //                // Optional: Handle unknown properties or log them
    //                break;
    //        }
    //    }

    //    habit.UpdatedAtUtc = DateTime.UtcNow;

    //    await dbContext.SaveChangesAsync();
    //    return NoContent();
    //}

}
