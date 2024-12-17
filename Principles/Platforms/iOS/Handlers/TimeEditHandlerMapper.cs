using CoreGraphics;
using DevExpress.Maui.Editors.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Platforms.iOS.Handlers
{
    public static class TimeEditHandlerMapper
    {
        public static void AddDoneButton( Microsoft.Maui.Handlers.ITimePickerHandler handler, Microsoft.Maui.ITimePicker timePicker )
        {
            var platformView = handler.PlatformView as UITextField;

            if (platformView?.InputView == null) return;

            UIToolbar toolbar = new UIToolbar( new CGRect( 0, 0, platformView.Frame.Size.Width, 44 ) )
            {
                BarStyle = UIBarStyle.Default,
                Translucent = true
            };

            UIBarButtonItem doneButton = new UIBarButtonItem( UIBarButtonSystemItem.Done, ( sender, e ) =>
            {
                platformView.ResignFirstResponder();
            } );

            toolbar.SetItems( new[] { new UIBarButtonItem( UIBarButtonSystemItem.FlexibleSpace ), doneButton }, true );

            platformView.InputAccessoryView = toolbar;
        }
    }
}
