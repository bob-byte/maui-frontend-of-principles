namespace Principles.Core.Services;

public interface IAreaOfLifeService
{
    Task<List<UserAreaOfLife>> UserAreasOfLife();
}