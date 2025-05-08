using DevExpress.Maui.Editors;

namespace Principles.Views.Templates;

public partial class NonYesCellTemplate
{
    public NonYesCellTemplate(CalendarCellData calendarCellData)
    {
        BindingContext = calendarCellData;
        InitializeComponent();
    }
}
