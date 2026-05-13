using backend.Models;

namespace backend.Repositories;

public interface IHolidayRepository
{
    Task<List<Holiday>> GetAllAsync();
    Task<Holiday?> GetByIdAsync(int id);
    Task<int> CreateAsync(Holiday holiday);
    Task DeleteAsync(int id);
    Task<List<Holiday>> GetActiveHolidaysAsync(DateTime date);
}
