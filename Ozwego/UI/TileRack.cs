using System;
using Windows.UI.Xaml.Controls;

namespace Ozwego.UI
{
    public class TileRack
    {
        private readonly Grid _theRack;

        private const int RowCount = 8;
        private const int ColumnCount = 3;
        private const int TileRackSize = RowCount * ColumnCount;

        private readonly bool[] _rackOccupency = new bool[TileRackSize];


        public TileRack(ref Grid grid)
        {
            _theRack = grid;

            for (int i = 0; i < TileRackSize; i++)
            {
                _rackOccupency[i] = false;
            }
        }


        public void AddTile(Grid newTile)
        {
            for (int i = 0; i < TileRackSize; i++)
            {
                if (_rackOccupency[i] == false)
                {
                    _rackOccupency[i] = true;

                    if (newTile.Parent != null)
                    {
                        var parent = newTile.Parent as Grid;

                        if (parent != null)
                        {
                            parent.Children.Remove(newTile);
                        }
                    }

                    newTile.SetValue(Grid.RowProperty, (int)Math.Floor(i / (double)ColumnCount));
                    newTile.SetValue(Grid.ColumnProperty, i % ColumnCount);

                    _theRack.Children.Add(newTile);

                    break;
                }
            }
        }


        public void RemoveTile(Grid tile)
        {
            var column = (int)tile.GetValue(Grid.ColumnProperty);
            var row = (int)tile.GetValue(Grid.RowProperty);

            int index = column + row*ColumnCount;

            _rackOccupency[index] = false;

            _theRack.Children.Remove(tile);
        }


        public void ClearTileRack()
        {
            _theRack.Children.Clear();
        }
    }
}
