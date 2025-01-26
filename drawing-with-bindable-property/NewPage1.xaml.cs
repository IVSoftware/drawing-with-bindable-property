using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepDiary.Controls;
using System.Collections;
using System.Collections.ObjectModel;

namespace SleepDiary
{
    public partial class NewPage1 : ContentPage
    {
        public NewPage1() => InitializeComponent();
    }
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
}
