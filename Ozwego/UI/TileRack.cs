using Windows.UI.Xaml.Controls;

namespace Ozwego.UI
{
    public class TileRack
    {
        private readonly Grid _theRack;

        private const int TileRackSize = 20;

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

                    newTile.SetValue(Grid.RowProperty, 0);
                    newTile.SetValue(Grid.ColumnProperty, i);

                    _theRack.Children.Add(newTile);

                    break;
                }
            }
        }

        public void RemoveTile(Grid tile)
        {
            var column = (int)tile.GetValue(Grid.ColumnProperty);

            _rackOccupency[column] = false;

            _theRack.Children.Remove(tile);
        }

        public void ClearTileRack()
        {
            _theRack.Children.Clear();
        }
    }
}
