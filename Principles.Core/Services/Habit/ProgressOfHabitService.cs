using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Principles.Core.Services;

public class ProgressOfHabitService : BaseEntityService<ProgressOfHabit>, IProgressOfHabitService
{
    private readonly INetworkService m_networkService;
    private readonly IProgressOfHabitRemoteApi m_remoteApi;

    public ProgressOfHabitService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_remoteApi = serviceProvider.GetRequiredService<IProgressOfHabitRemoteApi>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
    }

    public async Task UpdateAsync( ProgressOfHabit progressOfHabit )
    {
        progressOfHabit.LastModified = DateTime.UtcNow;
        await Database.SaveAsync( progressOfHabit ).ConfigureAwait( false );

        if (m_networkService.IsConnected)
        {
            try
            {
                await m_remoteApi.SaveAsync( progressOfHabit ).ConfigureAwait( false );
                return;
            }
            catch
            {
                // Queue below.
            }
        }

        await SyncQueueService.AddToQueueAsync( progressOfHabit, OperationKind.Save ).ConfigureAwait( false );
    }
}
