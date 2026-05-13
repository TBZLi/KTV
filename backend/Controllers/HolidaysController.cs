using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Repositories;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayRepository _holidayRepo;

    public HolidaysController(IHolidayRepository holidayRepo) => _holidayRepo = holidayRepo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var holidays = await _holidayRepo.GetAllAsync();
        return Ok(holidays);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayRequest request)
    {
        if (request.StartDate > request.EndDate) throw new Exception("开始日期不能晚于结束日期");
        if (request.VipMultiplier <= 0 || request.MediumMultiplier <= 0 || request.SmallMultiplier <= 0)
            throw new Exception("倍率必须大于 0");

        var holiday = new Holiday
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            VipMultiplier = request.VipMultiplier,
            MediumMultiplier = request.MediumMultiplier,
            SmallMultiplier = request.SmallMultiplier
        };

        var id = await _holidayRepo.CreateAsync(holiday);
        return Ok(new { id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _holidayRepo.DeleteAsync(id);
        return Ok();
    }
}

public record CreateHolidayRequest(
    DateTime StartDate,
    DateTime EndDate,
    decimal VipMultiplier,
    decimal MediumMultiplier,
    decimal SmallMultiplier
);
