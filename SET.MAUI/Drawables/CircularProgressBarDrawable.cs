using Microsoft.Maui.Graphics.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Drawables
{
    public class CircularProgressBarDrawable : BindableObject, IDrawable
    {
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create( nameof( Progress ), typeof( int ), typeof( CircularProgressBarDrawable ), defaultBindingMode: BindingMode.TwoWay );
        public static readonly BindableProperty SizeProperty = BindableProperty.Create( nameof( Size ), typeof( int ), typeof( CircularProgressBarDrawable ) );
        public static readonly BindableProperty ThicknessProperty = BindableProperty.Create( nameof( Thickness ), typeof( float ), typeof( CircularProgressBarDrawable ) );
        public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create( nameof( ProgressColor ), typeof( Color ), typeof( CircularProgressBarDrawable ) );
        public static readonly BindableProperty ProgressLeftColorProperty = BindableProperty.Create( nameof( ProgressLeftColor ), typeof( Color ), typeof( CircularProgressBarDrawable ) );
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create( nameof( TextColor ), typeof( Color ), typeof( CircularProgressBarDrawable ) );

        public int Progress
        {
            get => (int)GetValue( ProgressProperty );
            set => SetValue( ProgressProperty, value );
        }

        public int Size
        {
            get { return (int)GetValue( SizeProperty ); }
            set { SetValue( SizeProperty, value ); }
        }

        public float Thickness
        {
            get { return (float)GetValue( ThicknessProperty ); }
            set { SetValue( ThicknessProperty, value ); }
        }

        public Color ProgressColor
        {
            get { return (Color)GetValue( ProgressColorProperty ); }
            set { SetValue( ProgressColorProperty, value ); }
        }

        public Color ProgressLeftColor
        {
            get { return (Color)GetValue( ProgressLeftColorProperty ); }
            set { SetValue( ProgressLeftColorProperty, value ); }
        }

        public Color TextColor
        {
            get { return (Color)GetValue( TextColorProperty ); }
            set { SetValue( TextColorProperty, value ); }
        }

        public void Draw( ICanvas canvas, RectF dirtyRect )
        {
            float effectiveSize = Size - Thickness;
            float x = Thickness / 2;
            float y = Thickness / 2;

            if (Progress < 0)
            {
                Progress = 0;
            }
            else if (Progress > 100)
            {
                Progress = 100;
            }

            if (Progress < 100)
            {
                float angle = GetAngle( Progress );

                canvas.StrokeColor = ProgressLeftColor;
                canvas.StrokeSize = Thickness;
                canvas.DrawEllipse( x, y, effectiveSize, effectiveSize );

                // Draw arc
                canvas.StrokeColor = ProgressColor;
                canvas.StrokeSize = Thickness;
                canvas.DrawArc( x, y, effectiveSize, effectiveSize, 90, angle, true, false );
            }
            else
            {
                // Draw circle
                canvas.StrokeColor = ProgressColor;
                canvas.StrokeSize = Thickness;
                canvas.DrawEllipse( x, y, effectiveSize, effectiveSize );
            }

            // Make the percentage always the same size in relation to the size of the progress bar
            float fontSize = Progress == 100
                ? effectiveSize / 3.5f
                : effectiveSize / 2.86f;
            canvas.FontSize = fontSize;
            canvas.FontColor = TextColor;
            canvas.FillColor = TextColor;

            // Vertical text align the text, and we need a correction factor of around 1.15 to have it aligned properly
            // Note: The VerticalAlignment.Center property of the DrawString method seems to have no effect
            float verticalPosition = ((Size / 2) - (fontSize / 2)) * 0.25f;
            string progress = $"{Progress}%";
            string markdownText = @"**" + progress + @"%**";
            //IAttributedTextRun textRun = new AttributedTextRun()
            //IAttributedText attributedText = new AttributedText(progress, new ReadOnlyCollection(new List))
            canvas.DrawString( progress, x, verticalPosition, effectiveSize, effectiveSize, HorizontalAlignment.Center, VerticalAlignment.Center );
        }

        private float GetAngle( int progress )
        {
            float factor = 90f / 25f;

            if (progress > 75)
            {
                return -180 - ((progress - 75) * factor);
            }
            else if (progress > 50)
            {
                return -90 - ((progress - 50) * factor);
            }
            else if (progress > 25)
            {
                return 0 - ((progress - 25) * factor);
            }
            else
            {
                return 90 - (progress * factor);
            }
        }
    }
}
