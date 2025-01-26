using IVSoftware.Portable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepDiary.Controls
{
    class MyGraphicsView : GraphicsView, IDrawable
    {
        public MyGraphicsView() => Drawable = this;
        public static readonly BindableProperty ShapesProperty =
        BindableProperty.Create(
            nameof(Shapes),
            typeof(ObservableCollection<ShapeSD>),
            typeof(MyGraphicsView),
            new ObservableCollection<ShapeSD>(),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                if (bindable is MyGraphicsView view)
                {
                    view.Shapes.CollectionChanged -= localHandler;
                    view.Shapes.CollectionChanged += localHandler;
                    void localHandler(object? sender, NotifyCollectionChangedEventArgs e)=>
                    view.Refresh.StartOrRestart();
                }
            });

        public WatchdogTimer Refresh
        {
            get
            {
                if (_refresh is null)
                {
                    _refresh = new WatchdogTimer { Interval = TimeSpan.FromSeconds(0.1) };
                    _refresh.RanToCompletion += (sender, e) => Invalidate();
                }
                return _refresh;
            }
        }
        WatchdogTimer? _refresh = default;

        public ObservableCollection<ShapeSD> Shapes
        {
            get => (ObservableCollection<ShapeSD>)GetValue(ShapesProperty);
            set => SetValue(ShapesProperty, value);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.PaleGreen;
            canvas.FillRectangle(dirtyRect);

            foreach (var shape in Shapes)
            {
                if (shape is LineSD line)
                {
                    canvas.StrokeColor = Colors.Black;
                    canvas.DrawLine(line.X0, line.Y0, line.X1, line.Y1);
                }
            }
        }
    }

    // Sleep Diary Shapes
    public abstract class ShapeSD { }
    public class LineSD : ShapeSD
    {
        public float X0 { get; set; }
        public float X1 { get; set; }
        public float Y0 { get; set; }
        public float Y1 { get; set; }
        public Color Color { get; set; } = Colors.Black;
    }
}
