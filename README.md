I'm glad to see that the existing answer is moving things forward for you, and this is just something else to consider. Rather than perform the non-standard binding shown, it might be more idiomatic to make your own `MyGraphicsView` control that inherits `GraphicsView` and implements `IDrawable` directly. Then you can put the bindable properties in the view and have the property assignment follow a more typical pattern:

**XAML**
~~~xaml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SleepDiary.NewPage1"             
             xmlns:drawable="clr-namespace:SleepDiary"
             xmlns:controls="clr-namespace:SleepDiary.Controls">

    <ContentPage.BindingContext>
        <drawable:NewPage1ViewModel />
    </ContentPage.BindingContext>

    <ScrollView>
        <VerticalStackLayout
            WidthRequest="400"
            Padding="0" 
            Spacing="25">
            <Image
                Source="dotnet_bot.png"
                HeightRequest="185"
                Aspect="AspectFit"
                SemanticProperties.Description="dot net bot in a race car number eight" />            
            <Button Text="Draw" Command="{Binding DrawCommand}"/>
            <Button  Text="Clear" Command="{Binding ClearCommand}"/>
            <Border
                HeightRequest="300"
                WidthRequest="300"
                Stroke="#222222"
                StrokeThickness="2">
                <controls:MyGraphicsView 
                    Shapes="{Binding Shapes}" />
            </Border>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
~~~

___

The other thing is that your example shows an `int` that is your `testVariable`. And you got that working of course and that's great, but it only takes you so far. If you look at the xaml above, you will see that the "variable we are passing" is _now_ a collection of shapes rather than a single `int`. This collection forms a "document" that the custom control can draw when refreshed by way of `Invalidate`. And every time we add a new shape we restart a timer that invalidates the control when it expires in 100 ms.

**Custom GraphicsView**
~~~csharp
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
                canvas.DrawLine(line.X0, line.Y0, line.X1, line.Y1);
            }
        }
    }
    // <PackageReference Include="IVSoftware.Portable.WatchdogTimer" Version="1.2.1" />
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
~~~

___

**Custom Shapes (Minimal Example)**

~~~csharp
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
~~~
___

Let me get to the point. In your post, you asked:

>I would like to pass a variable into a GraphicView to update a drawing by, for example, pushing a button.

With the above in place, the View and ViewModel for a basic `NewPage1` might be implemented something like this, and we now can draw things by adding and removing various "shapes" from the `Shapes` collection.

~~~
public partial class NewPage1 : ContentPage
{
    public NewPage1() => InitializeComponent();
}

partial class NewPage1ViewModel : ObservableObject
{
    public IList Shapes { get; } = new ObservableCollection<ShapeSD>();

    // Handle the Draw button
    [RelayCommand]
    private void Draw()
    {
        // ...
    }

    // Handle the Clear button
    [RelayCommand]
    private void Clear()
    {
        Shapes.Clear();
    }
}
~~~

___

***DEMO***

Let's _"push a button and draw something."_

[![append to doc][1]][1]


~~~
partial class NewPage1ViewModel : ObservableObject
{
    public IList Shapes { get; } = new ObservableCollection<ShapeSD>();


    // Handle the Clear button
    [RelayCommand]
    private void Clear()
    {
        Shapes.Clear();
    }

    // Handle Draw Button
    [RelayCommand]
    private void Draw()
    {
        double centerX = CanvasSize / 2.0;
        double centerY = CanvasSize / 2.0;

        // Calculate the range of lines to draw for the current step
        int startIndex = (_currentStep * NumLines) / TotalSteps;
        int endIndex = ((_currentStep + 1) * NumLines) / TotalSteps;

        for (int i = startIndex; i < endIndex; i++)
        {
            double angle1 = Math.PI * 2 * i / NumLines;
            double angle2 = Math.PI * 2 * (i + 1) / NumLines;

            float x0 = (float)(centerX + Radius * Math.Cos(6 * angle1) * Math.Cos(angle1));
            float y0 = (float)(centerY + Radius * Math.Cos(6 * angle1) * Math.Sin(angle1));

            float x1 = (float)(centerX + Radius * Math.Cos(6 * angle2) * Math.Cos(angle2));
            float y1 = (float)(centerY + Radius * Math.Cos(6 * angle2) * Math.Sin(angle2));

            Shapes.Add(new LineSD
            {
                X0 = x0,
                Y0 = y0,
                X1 = x1,
                Y1 = y1,
                Color = GetGradientColor(i, NumLines),
            });
        }

        _currentStep = (_currentStep + 1) % TotalSteps;
    }

    private Color GetGradientColor(int index, int total)
    {
        // Create a smooth gradient effect by interpolating colors
        float t = (float)index / total;
        return Color.FromRgb(
            (byte)(255 * (1 - t)),
            (byte)(255 * t),
            (byte)(255 * (0.5 + 0.5 * Math.Cos(2 * Math.PI * t)))
        );
    }
    private int _currentStep = 0;
    private const int TotalSteps = 3;
    private const int CanvasSize = 300;
    private const int Radius = 150;
    private const int NumLines = 360;
}
~~~


  [1]: https://i.sstatic.net/51dtjw6H.png