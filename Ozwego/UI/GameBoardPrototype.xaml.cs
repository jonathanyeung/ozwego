using System;
using System.Collections.Generic;
using Ozwego.Common;
using Ozwego.Gameplay;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Ozwego.ViewModels;
using Ozwego.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Ozwego.UI
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GameBoardPrototype
    {
        private const int BorderWidth = 2;

        private const int BoxWidth = 50 - 2 * BorderWidth;

        private const int GameBoardDimension = 31;  // This value should be odd! Remember to update corresponding value in GameBoard.cs
        private const int StartingBoardDimension = 11; // This value should be odd!

        private int _northLimit;
        private int _southLimit;
        private int _eastLimit;
        private int _westLimit;

        private readonly TileRack _tileRack;

        private readonly PlayerPane _playerPane;

        // UI grid representation of the game board.
        private readonly Grid[,] _gameBoardArray;

        private readonly List<Rectangle> _dirtyTiles;

        private readonly List<Border> _gridBorders = new List<Border>();

        private readonly List<Grid> _playedTiles;

        public GameBoardPrototype()
        {
            InitializeComponent();

            DataContext = GameBoardViewModel.GetInstance();

            _tileRack = new TileRack(ref TileRackUi);
            
            _playerPane = new PlayerPane();
            PlayerPanel.ItemsSource = _playerPane.PlayerList;

            _gameBoardArray = new Grid[GameBoardDimension, GameBoardDimension];
            _dirtyTiles = new List<Rectangle>();

            GenerateGrid();
            _playedTiles = new List<Grid>();

            UiTester();
        }


        #region GameController Event Handlers

        //ToDo: Add a OnDumpEventReceived method.
        private void OnPeelEventReceived(object sender, string tileContents, string actionSender)
        {
            var tile = CreateLetterTile(tileContents);
            _tileRack.AddTile(tile);

            PlayPeelAnimation(actionSender);
        }


        /// <summary>
        /// Callback to highlight incorrect words.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="invalidPoints"></param>
        private void OnInvalidWordUiUpdateEvent(object sender, List<Point> invalidPoints)
        {
            var fadeIncorrectWordsToRed = new Storyboard();

            foreach (var point in invalidPoints)
            {
                Rectangle rect = null;

                if (_gameBoardArray[(int)point.X, (int)point.Y] != null)
                {
                    rect = _gameBoardArray[(int)point.X, (int)point.Y].Children[0] as Rectangle;
                }

                if (rect != null)
                {
                    rect.Fill = new SolidColorBrush(Colors.Crimson);

                    //ToDo: Make this fucking animation not throw an exception.
                    //var fadeToRedAnimation = new ColorAnimation();
                    //fadeToRedAnimation.To = (Colors.Red);
                    //fadeToRedAnimation.Duration = TimeSpan.FromSeconds(1);
                    //fadeToRedAnimation.EnableDependentAnimation = true;
                    //Storyboard.SetTarget(FadeIncorrectWordsToRed, rect);
                    //try
                    //{
                    //    Storyboard.SetTargetProperty(fadeToRedAnimation, "(Rectangle.Fill).(SolidColorBrush.Color)");
                    //}
                    //catch (Exception)
                    //{

                    //}
                    //FadeIncorrectWordsToRed.Children.Add(fadeToRedAnimation);

                    _dirtyTiles.Add(rect);
                }
            }

            fadeIncorrectWordsToRed.Begin();
        }


        private void OnGameStartedEvent(object sender)
        {
            GenerateBoxes();
            ColorCircle.Begin();

            _playerPane.Initialize();

            StartGameAnimation.Begin();
            StartGameAnimation.Completed += StartGameAnimation_Completed;
        }

        void StartGameAnimation_Completed(object sender, object e)
        {
            // ToDo: On the animation complete, signal an event to the game controller to start the bots (look at GameController.StartGame method).
        }

        #endregion


        #region Helper Methods

        private void UiTester()
        {
            //GameBoardScrollViewer.ZoomSnapPoints.Add(.5f);
            //GameBoardScrollViewer.ZoomSnapPoints.Add(1.0f);
            //GameBoardScrollViewer.ZoomSnapPoints.Add(2.5f);
            //GameBoardScrollViewer.ZoomSnapPointsType = SnapPointsType.Mandatory;
            GameBoardScrollViewer.Background = new SolidColorBrush(Colors.RoyalBlue) {Opacity = 0.1f};

            DumpCircle.Fill.Opacity = .7f;
            //TileRackUi.Background = new SolidColorBrush(Colors.MediumVioletRed);
            //TileRackUi.Background.Opacity = .4f;

            //Storyboard.SetTarget(GridFadeIn, TestRect);
            //Storyboard.SetTarget(GridFadeOut, TestRect);


            //Storyboard.SetTargetProperty(GridFadeIn, "(Fill.Opacity)");
            //Storyboard.SetTargetProperty(GridFadeOut, "(Fill.Opacity)");

            //TilePoolCountUI.SetBinding(TextBlock.TextProperty, 

            Storyboard.SetTarget(ColorCircle, DumpCircle);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">index of child element on which to perform the animation.</param>
        private void PlayPeelAnimation(string actionSender)
        {
            PlayPlayerActionAnimation(actionSender, "Peeled!");
        }

        private void PlayDumpAnimation(string actionSender)
        {
            PlayPlayerActionAnimation(actionSender, "Dumped!");
        }


        /// <summary>
        /// ToDo: Refactor this method.  This was created as an ugly temp solution.
        /// </summary>
        /// <param name="index">index of child element on which to perform the animation</param>
        /// <param name="actionTextString">The string to update the action status text box with</param>
        private void PlayPlayerActionAnimation(string actionSender, string actionTextString)
        {
            //
            // Find the right item template instance from the string of the name of the Action Sender.
            //

            int index = 0;
            PlayerPaneViewModel curViewModel = null;
            foreach (PlayerPaneViewModel viewModel in _playerPane.PlayerList)
            {
                if (viewModel.PlayerName == actionSender)
                {
                    curViewModel = viewModel;
                    break;
                }

                index++;
            }

            if (null == curViewModel)
            {
                return;
            }

            var animationTargetContainer = PlayerPanel.ItemContainerGenerator.ContainerFromIndex(index);


            //
            // ToDo: This code is completely dependent on the XAML design of the PlayerPane item
            // template.  If that changes, the code below will break!  Find a stronger contract
            // here.
            //

            var grid = VisualTreeHelper.GetChild(animationTargetContainer, 0);
            var stackPanel = VisualTreeHelper.GetChild(grid, 2);
            var actionString = VisualTreeHelper.GetChild(stackPanel, 2) as TextBlock;

            actionString.Text = actionTextString;

            var actionStringAnimation = new DoubleAnimation();
            actionStringAnimation.From = 0.0f;
            actionStringAnimation.To = 1.0f;
            actionStringAnimation.Duration = TimeSpan.FromSeconds(1);

            Storyboard MyStoryBoard = new Storyboard();
            Storyboard.SetTarget(actionStringAnimation, actionString);
            Storyboard.SetTargetProperty(actionStringAnimation, "Opacity");

            MyStoryBoard.Children.Add(actionStringAnimation);

            MyStoryBoard.Begin();
        }


        private void GenerateBoxes()
        {
            _tileRack.ClearTileRack();

            var gameController = GameController.GetInstance();
            var startingTiles = gameController.GetTileRack();

            for (var i = 0; i < startingTiles.Count; i++)
            {
                var boxOne = CreateLetterTile(startingTiles[i].TileContents);
                _tileRack.AddTile(boxOne);
            }
        }


        private void GenerateGrid()
        {
            for (int i = 0; i < StartingBoardDimension; i++)
            {
                var row = new RowDefinition { Height = new GridLength(50) };
                GameBoard.RowDefinitions.Add(row);

                var column = new ColumnDefinition { Width = new GridLength(50) };
                GameBoard.ColumnDefinitions.Add(column);
            }

            for (var i = 0; i < StartingBoardDimension; i++)
            {
                for (var j = 0; j < StartingBoardDimension; j++)
                {
                    CreateBorder(i, j);

                    /* Animation Code:
                    Storyboard.SetTarget(GridFadeOut, border);
                    Storyboard.SetTarget(GridFadeIn, border);


                    var newFadeOutAnimation = new DoubleAnimation();
                    newFadeOutAnimation.From = 1.0f;
                    newFadeOutAnimation.To = 0.0f;
                    newFadeOutAnimation.Duration = TimeSpan.FromSeconds(.25);

                    var newFadeInAnimation = new DoubleAnimation();
                    newFadeInAnimation.From = 0.0f;
                    newFadeInAnimation.To = 1.0f;
                    newFadeInAnimation.Duration = TimeSpan.FromSeconds(.25);

                    Storyboard.SetTarget(newFadeInAnimation, border);
                    Storyboard.SetTarget(newFadeOutAnimation, border);

                    Storyboard.SetTargetProperty(newFadeInAnimation, "(BorderBrush.Opacity)");
                    Storyboard.SetTargetProperty(newFadeOutAnimation, "(BorderBrush.Opacity)");

                    GridFadeIn.Children.Add(newFadeInAnimation);
                    GridFadeOut.Children.Add(newFadeOutAnimation);
                    */
                }
            }

            //
            // Set the boundaries of the grid.
            //
            _northLimit = _westLimit = (GameBoardDimension / 2) - (StartingBoardDimension - 1) / 2;
            _southLimit = _eastLimit = (GameBoardDimension / 2) + (StartingBoardDimension - 1) / 2;

        }


        //
        //ToDo:?  private void ShrinkGridIfRequired(int x, int y)
        //


        private void ExpandGridIfRequired(int x, int y)
        {
            //
            // The limits are absolute coordinates, while the incoming x and y are relative
            // coordinates.  When adjusting the grid column/row values, absolute coordinates
            // must be used.  When dealing with limits, relative coordinates must be used.
            //

            const int boardBufferSize = 3;
            var absX = x + _westLimit;
            var absY = y + _northLimit;


            //
            // Case: Expand in the East direction
            //

            while ((absX + boardBufferSize > _eastLimit) && (_eastLimit < GameBoardDimension - 1))
            {
                var column = new ColumnDefinition { Width = new GridLength(50) };
                GameBoard.ColumnDefinitions.Add(column);
                _eastLimit++;

                for (int i = 0; i <= _southLimit - _northLimit; i++)
                {
                    CreateBorder(_eastLimit - _westLimit, i);
                }
            }


            //
            // Case: Expand in the West direction
            //

            while ((absX - boardBufferSize < _westLimit) && (_westLimit > 0))
            {
                var column = new ColumnDefinition { Width = new GridLength(50) };
                GameBoard.ColumnDefinitions.Insert(0, column);


                //
                // Shift everything one position in the East direction.
                //

                foreach (var gridBorder in _gridBorders)
                {
                    var col = (int)gridBorder.GetValue(Grid.ColumnProperty);
                    gridBorder.SetValue(Grid.ColumnProperty, col + 1);
                }

                foreach (var tile in _playedTiles)
                {
                    var col = (int)tile.GetValue(Grid.ColumnProperty);
                    tile.SetValue(Grid.ColumnProperty, col + 1);
                }


                _westLimit--;

                for (int i = 0; i <= _southLimit - _northLimit; i++)
                {
                    CreateBorder(0, i);
                }
            }


            //
            // Case: Expand in the South direction
            //

            while ((absY + boardBufferSize > _southLimit) && (_southLimit < GameBoardDimension - 1))
            {
                var row = new RowDefinition { Height = new GridLength(50) };
                GameBoard.RowDefinitions.Add(row);
                _southLimit++;

                for (int i = 0; i <= _eastLimit - _westLimit; i++)
                {
                    CreateBorder(i, _southLimit - _northLimit);
                }
            }


            //
            // Case: Expand in the North direction
            //

            while ((absY - boardBufferSize < _northLimit) && (_northLimit > 0))
            {
                var row = new RowDefinition { Height = new GridLength(50) };
                GameBoard.RowDefinitions.Insert(0, row);


                //
                // Shift everything one position in the South direction.
                //

                foreach (var gridBorder in _gridBorders)
                {
                    var newRow = (int)gridBorder.GetValue(Grid.RowProperty);
                    gridBorder.SetValue(Grid.RowProperty, newRow + 1);
                }

                foreach (var tile in _playedTiles)
                {
                    var newRow = (int)tile.GetValue(Grid.RowProperty);
                    tile.SetValue(Grid.RowProperty, newRow + 1);
                }


                _northLimit--;

                for (int i = 0; i <= _eastLimit - _westLimit; i++)
                {
                    CreateBorder(i, 0);
                }
            }
        }


        private Grid CreateLetterTile(string tileContents)
        {
            var boxOne = new Grid
            {
                Width = BoxWidth,
                Height = BoxWidth,
                ManipulationMode = ManipulationModes.All
            };

            boxOne.ManipulationDelta += Box_ManipulationDelta;
            boxOne.ManipulationCompleted += BoxOnManipulationCompleted;
            boxOne.ManipulationStarted += BoxOnManipulationStarted;
            boxOne.ManipulationInertiaStarting += Box_ManipulationInertiaStarting;
            boxOne.RenderTransform = new TranslateTransform();

            var rectOne = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.DodgerBlue),
                Width = BoxWidth,
                Height = BoxWidth
            };

            var textOne = new TextBlock
            {
                FontSize = 38,
                Text = tileContents,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            boxOne.Children.Add(rectOne);
            boxOne.Children.Add(textOne);

            return boxOne;
        }


        private void CreateBorder(int x, int y)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(BorderWidth),
                BorderBrush = new SolidColorBrush(Colors.DarkGray)

            };

            border.BorderBrush.Opacity = .3f;

            border.SetValue(Grid.RowProperty, y);
            border.SetValue(Grid.ColumnProperty, x);
            GameBoard.Children.Add(border);
            _gridBorders.Add(border);
        }


        private double distanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        #endregion


        #region Box Manipulation Handlers

        private void BoxOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var box = sender as Grid;

            if (box == null)
            {
                return;
            }

            //GridFadeIn.Begin();


            //
            // If there are any tiles that have been painted red, change them back to blue.
            //

            foreach (var dirtyRect in _dirtyTiles)
            {
                dirtyRect.Fill = new SolidColorBrush(Colors.DodgerBlue);
            }
            _dirtyTiles.Clear();


            //
            // If we're moving a tile from one position on the game board to another, then 
            // clear the game board spot from which we are moving the tile from.
            //

            if (box.Parent == GameBoard)
            {
                var curColumn = (int)box.GetValue(Grid.ColumnProperty);
                var curRow = (int)box.GetValue(Grid.RowProperty);

                var gameController = GameController.GetInstance();
                gameController.ClearBoardSpace(_westLimit + curColumn, _northLimit + curRow);

                _playedTiles.Remove(box);
            }

            var rect = box.Children[0] as Rectangle;
            if (rect != null)
            {
                rect.Fill = new SolidColorBrush(Colors.Crimson);
            }

            //box.SetValue(Canvas.ZIndexProperty, 3);

            e.Handled = true;
        }


        private void Box_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var dragableItem = sender as Grid;
            if (dragableItem == null)
            {
                return;
            }

            var translateTransform = dragableItem.RenderTransform as TranslateTransform;

            float intendedZoomFactor;


            //
            // Do not modify the scroll speed if the tile is being dragged from the tile rack, as 
            // that will always have a magnification factor of 1.
            //

            intendedZoomFactor = ((Grid)dragableItem.Parent == TileRackUi)
                ? 1
                : GameBoardScrollViewer.ZoomFactor;


            //
            // The drag speed needs to be scaled with the inverse of the magnification level of the
            // scroll viewer.
            //

            if (translateTransform != null)
            {
                translateTransform.X += (e.Delta.Translation.X * (1 / intendedZoomFactor));
                translateTransform.Y += (e.Delta.Translation.Y * (1 / intendedZoomFactor));
            }

            e.Handled = true;
        }


        private void Box_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            // ToDo: Define these magic numbers. Look at desiredDeceleration property.
            e.TranslationBehavior.DesiredDeceleration = 500.0 * 96.0 / (1000.0 * 1000.0);
            e.Handled = true;
        }


        private async void BoxOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var gameController = GameController.GetInstance();
            //GridFadeOut.Begin();

            double smallestDistance = 0;
            int closestGridRow = 0;
            int closestGridColumn = 0;

            var box = sender as Grid;
            if (box == null)
            {
                return;
            }

            var rect = box.Children[0] as Rectangle;
            var textBlock = box.Children[1] as TextBlock;
            if (rect == null || textBlock == null)
            {
                return;
            }

            var translateTransform = box.RenderTransform as TranslateTransform;

            rect.Fill = new SolidColorBrush(Colors.DodgerBlue);


            //foreach (var border in _gridBorders)
            //{
            //    border.Visibility = Visibility.Collapsed;
            //}


            //
            // Get the screen position of the various UI elements at the point of the release
            // of the box.
            //

            var gt = box.TransformToVisual(pageRoot);
            var endOfDragPosition = gt.TransformPoint(new Point(0, 0));

            gt = TileRackUi.TransformToVisual(pageRoot);
            var tileRackAnchorPoint = gt.TransformPoint(new Point(0, 0));

            gt = DumpBox.TransformToVisual(pageRoot);
            var dumpBoxAnchorPoint = gt.TransformPoint(new Point(0, 0));


            //
            // If the user has put the tile back on the tile rack:
            //

            if (   (endOfDragPosition.X > tileRackAnchorPoint.X) 
                && (tileRackAnchorPoint.X + TileRackUi.ActualWidth > endOfDragPosition.X)
                && (endOfDragPosition.Y > tileRackAnchorPoint.Y)
                && (tileRackAnchorPoint.Y + TileRackUi.ActualHeight > endOfDragPosition.Y))
                {
                    if (box.Parent == TileRackUi)
                    {
                        _tileRack.RemoveTile(box);
                        gameController.RemoveTileFromRack(textBlock.Text);
                    }

                    if (translateTransform != null)
                    {
                        translateTransform.X = 0;
                        translateTransform.Y = 0;
                    }

                    _tileRack.AddTile(box);
                    gameController.ReturnTileToRack(textBlock.Text);
                }


            //
            // Check if the user is trying to dump the tile into the tile pool:
            //

            else if ((endOfDragPosition.X > dumpBoxAnchorPoint.X) 
                && (dumpBoxAnchorPoint.X + DumpBox.ActualWidth > endOfDragPosition.X)
                && (endOfDragPosition.Y > dumpBoxAnchorPoint.Y)
                && (dumpBoxAnchorPoint.Y + DumpBox.ActualHeight > endOfDragPosition.Y))
            {
                var returnedTile = new Tile(textBlock.Text);


                //ToDo: The line below is super hacky...yuck yuck.  Should not use app.clientbuddyinstance here, but instead use the performdumpaction method of humanplayer.cs
                var tiles = await gameController.PerformDumpAction(Settings.Alias, returnedTile);


                //
                // If there aren't enough tiles left for a dump, then return the tile back to the 
                // rack.
                //

                if (tiles.Count == 0)
                {
                    //ToDo: This code was copied from the above "user has put the tile back on the tile rack section.  Consolidate this.
                    if (box.Parent == TileRackUi)
                    {
                        _tileRack.RemoveTile(box);
                        gameController.RemoveTileFromRack(textBlock.Text);
                    }

                    if (translateTransform != null)
                    {
                        translateTransform.X = 0;
                        translateTransform.Y = 0;
                    }

                    _tileRack.AddTile(box);
                    gameController.ReturnTileToRack(textBlock.Text);
                }

                else
                {
                    if (box.Parent == TileRackUi)
                    {
                        _tileRack.RemoveTile(box);
                        gameController.RemoveTileFromRack(textBlock.Text);
                    }

                    foreach (var tile in tiles)
                    {
                        var newTile = CreateLetterTile(tile.TileContents);
                        _tileRack.AddTile(newTile);
                        gameController.ReturnTileToRack(tile.TileContents);
                    }

                    box.Visibility = Visibility.Collapsed;
                }
            }


            //
            // Else, assume that the user is trying to put the tile onto the game board.
            // Iterate through all the squares of the grid to determine which is closest to the box.
            // Also, ensure that the tiles don't overlap.
            // ToDo: Optimize the lookup algorithm here.
            //

            else
            {
                foreach (Border b in _gridBorders)
                {
                    gt = b.TransformToVisual(pageRoot);
                    var gridAnchorPoint = gt.TransformPoint(new Point(0, 0));

                    var curGridRow = (int)b.GetValue(Grid.RowProperty);
                    var curGridColumn = (int)b.GetValue(Grid.ColumnProperty);

                    if ((smallestDistance > distanceBetweenPoints(endOfDragPosition, gridAnchorPoint)
                        || (smallestDistance == 0))
                        && (!gameController.IsBoardSpaceOccupied(_westLimit + curGridColumn, _northLimit + curGridRow)))
                    {
                        smallestDistance = distanceBetweenPoints(endOfDragPosition, gridAnchorPoint);
                        closestGridRow = curGridRow;
                        closestGridColumn = curGridColumn;
                    }
                }

                if (box.Parent == TileRackUi)
                {
                    _tileRack.RemoveTile(box);
                    gameController.RemoveTileFromRack(textBlock.Text);
                }

                //
                // Set the coordinates of the rectangle to the closest grid space.
                //

                if (translateTransform != null)
                {
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                }

                box.SetValue(Grid.RowProperty, closestGridRow);
                box.SetValue(Grid.ColumnProperty, closestGridColumn);
                //box.SetValue(Canvas.ZIndexProperty, 1);

                _gameBoardArray[_westLimit + closestGridColumn, _northLimit + closestGridRow] = box;


                //
                // Get the box's letter value and then update it in the gameboard array.
                //

                _playedTiles.Add(box);

                gameController.SetBoardSpace(textBlock.Text, _westLimit + closestGridColumn, _northLimit + closestGridRow);

                ExpandGridIfRequired(closestGridColumn, closestGridRow);

                if (box.Parent != GameBoard)
                {
                    GameBoard.Children.Add(box);
                }

                gameController.OnTilePlayedbyHumanPlayer();
            }

            e.Handled = true;
        }


        private void GameBoardScrollViewer_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var zoomPoint = e.GetPosition(GameBoardScrollViewer);
            GameBoardScrollViewer.ScrollToHorizontalOffset(zoomPoint.X);
            GameBoardScrollViewer.ScrollToVerticalOffset(zoomPoint.Y);

            GameBoardScrollViewer.ZoomToFactor(GameBoardScrollViewer.ZoomFactor > 1.5f ? .5f : 2.0f);
        }

        #endregion


        #region Button EH

        private void StartGame_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //PlayActionStringAnimation();
            //var gameController = GameController.GetInstance();

            //if (!gameController.GameStarted)
            //{
            //    gameController.StartGame();
            //}
        }


        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            GridFadeOut.Begin();
        }


        private void UIElement_OnTapped2(object sender, TappedRoutedEventArgs e)
        {
            GridFadeIn.Begin();
        }

        #endregion


        #region overrides

        protected override async void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            //
            // Hook up the event handlers.
            //

            var gameController = GameController.GetInstance();
            
            gameController.PeelEvent += OnPeelEventReceived;
            
            gameController.InvalidWordUiUpdateEvent += OnInvalidWordUiUpdateEvent;
            
            gameController.GameStartedEvent += OnGameStartedEvent;


            //
            // Initialize the game.  If the game was started in local mode, then go ahead and start
            // the game.  If we're in online mode, then wait for the server to indicate game start
            // before starting.
            //

            var args = (GameBoardNavigationArgs)e.Parameter;


            await gameController.Initialize(args);

            if (args.GameConnectionType == GameConnectionType.Local)
            {
                if (!gameController.GameStarted)
                {
                    gameController.StartGame();
                }
            }
        }


        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var gameController = GameController.GetInstance();
            gameController.PeelEvent -= OnPeelEventReceived;
            gameController.InvalidWordUiUpdateEvent -= OnInvalidWordUiUpdateEvent;
            gameController.GameStartedEvent -= OnGameStartedEvent;
            gameController.Cleanup();
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #endregion

    }
}
