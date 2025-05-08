using DevExpress.Maui.Editors;

namespace Principles.Views.Templates;

public partial class YesAutoCellTemplate
{
    public YesAutoCellTemplate(CalendarCellData calendarCellData)
    {
        BindingContext = calendarCellData;
        InitializeComponent();
    }
}
