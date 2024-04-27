namespace SET.Core.Services;

public interface IAreaOfLifeService
{
    Task<List<UserAreaOfLife>> UserAreasOfLife();
}