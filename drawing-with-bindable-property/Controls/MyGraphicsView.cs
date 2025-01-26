using IVSoftware.Portable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.PaleGreen;
            canvas.FillRectangle(dirtyRect);

            foreach (var shape in Shapes)
            {
                if (shape is LineSD line)
                {
                    canvas.StrokeColor = shape.Color ?? Colors.Black;
                    canvas.StrokeSize = 2F;
                    canvas.DrawLine(line.X0, line.Y0, line.X1, line.Y1);
                }
            }
        }

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
    }

    // Sleep Diary Shapes
    public abstract class ShapeSD 
    { 
        public Color? Color { get; set; }
    }
    public class LineSD : ShapeSD
    {
        public float X0 { get; set; }
        public float X1 { get; set; }
        public float Y0 { get; set; }
        public float Y1 { get; set; }
    }
}
