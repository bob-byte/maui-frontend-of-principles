using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;
public interface IReminderService
{
    Task<Reminder> HabitsReportReminderAsync();
    Task<DtoWithId> SaveHabitsReportReminderAsync( Reminder reminder );
    Task<AllRemindersResponse> LoadAllRemindersAsync();
}
