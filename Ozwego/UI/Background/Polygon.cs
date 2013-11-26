using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Ozwego.UI.Background
{
    public class Polygon
    {
        public List<Windows.UI.Xaml.Shapes.Polygon> UiPolygons;

        private readonly int _row;
        private readonly int _column;

        const int UnitLength = 100;
        const int HalfUnitLength = 50;


        public Polygon(int row, int column)
        {
            UiPolygons = new List<Windows.UI.Xaml.Shapes.Polygon>();

            _row = row;
            _column = column;

            Draw();
        }


        private void CreateVertices(Windows.UI.Xaml.Shapes.Polygon polygon, int aX, int aY, int bX, int bY, int cX, int cY)
        {
            polygon.Points = new PointCollection {new Point(aX, aY), new Point(bX, bY), new Point(cX, cY)};
        }

        private void Draw()
        {
            Windows.UI.Xaml.Shapes.Polygon UiPolygon;

            //
            // Top Triangle.
            //

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, 0, 0, 0, HalfUnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, 0, 0, HalfUnitLength, 0, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, HalfUnitLength, 0, UnitLength, 0, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, 0, HalfUnitLength, 0, UnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);


            //
            // Bottom Triangle
            //

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, 0,UnitLength, HalfUnitLength,UnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, UnitLength, UnitLength, HalfUnitLength, UnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, UnitLength, UnitLength, UnitLength, HalfUnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);

            UiPolygon = new Windows.UI.Xaml.Shapes.Polygon();
            CreateVertices(UiPolygon, UnitLength, 0, UnitLength, HalfUnitLength, HalfUnitLength, HalfUnitLength);
            SetPolygonProperties(UiPolygon);
            UiPolygons.Add(UiPolygon);
        }


        private void SetPolygonProperties(Windows.UI.Xaml.Shapes.Polygon polygon)
        {
            polygon.SetValue(Grid.RowProperty, _row);
            polygon.SetValue(Grid.ColumnProperty, _column);

            polygon.Fill = ColorGenerator.GetRandomColor(ColorScheme.Purple);
            polygon.Opacity = .3f;
        }
    }
}
