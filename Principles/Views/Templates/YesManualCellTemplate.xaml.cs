using DevExpress.Maui.Editors;

namespace Principles.Views.Templates;

public partial class YesManualCellTemplate
{
    public YesManualCellTemplate(CalendarCellData calendarCellData)
    {
        BindingContext = calendarCellData;
        InitializeComponent();
    }
}
