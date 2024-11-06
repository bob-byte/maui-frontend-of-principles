using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;
public class EditedGeneralReminder
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsEnabled { get; set; }
    public int UserNotificationRequestId { get; set; }
}
