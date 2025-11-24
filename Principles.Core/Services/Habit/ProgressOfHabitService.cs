using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Principles.Core.Services;

public class ProgressOfHabitService : BaseEntityService<ProgressOfHabit>, IProgressOfHabitService
{
    private IProgressOfHabitRemoteApi m_remoteApi;

    public ProgressOfHabitService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_remoteApi = serviceProvider.GetRequiredService<IProgressOfHabitRemoteApi>();
    }

    public async Task UpdateAsync( ProgressOfHabit progressOfHabit )
    {
        await LocalRemoteExecutor.ExecuteAsync<ProgressOfHabit>(
            localCall: () => Database.SaveAsync( progressOfHabit ),
            remoteCall: () => m_remoteApi.SaveAsync( progressOfHabit ),
            operation: OperationKind.Save,
            data: progressOfHabit
        );
    }
}
