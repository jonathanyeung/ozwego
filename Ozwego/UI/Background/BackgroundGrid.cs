using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Ozwego.UI.Background
{
    public class BackgroundGrid
    {
        public Grid PolygonGrid;

        private DispatcherTimer timer;

        private Storyboard colorStoryboard;

        private static BackgroundGrid _instance;


        public static BackgroundGrid GetInstance()
        {
            return _instance ?? (_instance = new BackgroundGrid());
        }


        public void BeginSubtleAnimation()
        {
            timer.Stop();
            colorStoryboard.Begin();
        }


        public void BeginFlashAnimation()
        {
            colorStoryboard.Stop();
            timer.Start();
        }


        private BackgroundGrid()
        {
            InitializeTimer();

            colorStoryboard = new Storyboard();

            //ToDo: Re-enable this.  Currently this is causing memory leaks.
            //colorStoryboard.Completed += (sender, o) =>
            //{
            //    foreach (var child in colorStoryboard.Children)
            //    {
            //        (child as ColorAnimation).To = ColorGenerator.GetRandomColor(ColorScheme.Purple).Color;
            //    }
            //    colorStoryboard.Begin();
            //};

            PolygonGrid = GenerateGrid();

            DrawPolygonBackground();
        }


        private Grid GenerateGrid()
        {
            var bounds = Window.Current.Bounds;

            var height = bounds.Height;

            var width = bounds.Width;

            var columnCount = Math.Ceiling(width / 100);
            var rowCount = Math.Ceiling(height / 100);

            var grid = new Grid();

            for (var i = 0; i < columnCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
            }

            for (var i = 0; i < rowCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100) });
            }

            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.HorizontalAlignment = HorizontalAlignment.Center;

            //BackgroundGrid.Children.Add(grid);

            return grid;
        }


        private void DrawPolygonBackground()
        {
            colorStoryboard.Stop();
            colorStoryboard.Children.Clear();

            for (int i = 0; i < PolygonGrid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < PolygonGrid.ColumnDefinitions.Count; j++)
                {
                    var newpoly = new Polygon(i, j);

                    foreach (var polygon in newpoly.UiPolygons)
                    {
                        PolygonGrid.Children.Add(polygon);

                        var animation = CreatePolygonAnimation();
                        Storyboard.SetTarget(animation, polygon);
                        Storyboard.SetTargetProperty(animation, "(Polygon.Fill).(SolidColorBrush.Color)");

                        colorStoryboard.Children.Add(animation);
                    }
                }
            }
        }


        private void InitializeTimer()
        {
            timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1000)};
            timer.Tick += (sender, o) => ChangeTileColor();
        }


        private ColorAnimation CreatePolygonAnimation()
        {
            var animation = new ColorAnimation
                {
                    AutoReverse = false,
                    EnableDependentAnimation = true,
                    Duration = TimeSpan.FromMilliseconds(4000),
                    To = ColorGenerator.GetRandomColor(ColorScheme.Purple).Color,
                    EasingFunction = new ExponentialEase()
                };

            return animation;
        }


        private ColorAnimation CreatePolygonTransformAnimation()
        {
            //var anim = new animation
            var animation = new ColorAnimation
            {
                AutoReverse = false,
                EnableDependentAnimation = true,
                Duration = TimeSpan.FromMilliseconds(4000),
                To = ColorGenerator.GetRandomColor(ColorScheme.Purple).Color
            };

            animation.EasingFunction = new ExponentialEase();
            return animation;
        }


        private void ChangeTileColor()
        {
            foreach (var child in PolygonGrid.Children)
            {
                (child as Windows.UI.Xaml.Shapes.Polygon).Fill = ColorGenerator.GetRandomColor(ColorScheme.Purple);
            }
        }
    }
}
