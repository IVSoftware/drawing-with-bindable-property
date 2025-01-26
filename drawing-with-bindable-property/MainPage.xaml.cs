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

        [RelayCommand]
        private void Draw()
        {
            Shapes.Add(new LineSD { X0 = 50, Y0 = 50, X1 = 350, Y1 = 50 });
            Shapes.Add(new LineSD { X0 = 50, Y0 = 100, X1 = 350, Y1 = 100 });
            Shapes.Add(new LineSD { X0 = 50, Y0 = 150, X1 = 350, Y1 = 150 });
        }

        [RelayCommand]
        private void Clear()
        {
            Shapes.Clear();
        }
    }
}
