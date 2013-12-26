using System;
using System.Collections.Generic;
using Ozwego.Common;
using Ozwego.Gameplay;
using Ozwego.Server;
using Shared;
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
using Ozwego.UI.Background;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Ozwego.UI
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GameBoardPrototype
    {
        private BackgroundGrid _background;

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


            //
            // Background grid initialization
            //

            _background = new BackgroundGrid();
            _background.PolygonGrid.SetValue(Grid.ColumnSpanProperty, 3);

            RootGrid.Children.Insert(0, _background.PolygonGrid);

            //_background.BeginSubtleAnimation();
        }


        #region GameController Event Handlers

        //ToDo: Add a OnDumpEventReceived method.
        private void OnPeelEventReceived(object sender, string tileContents, string actionSender)
        {
            var tile = CreateLetterTile(tileContents);
            _tileRack.AddTile(tile);

            PlayCompletePeelAnimation(actionSender);
        }


        /// <summary>
        /// Callback to highlight incorrect words.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="invalidPoints"></param>
        private void OnInvalidWordUiUpdateEvent(object sender, List<Point> invalidPoints)
        {
            var fadeIncorrectWordsToRed = new Storyboard();

            List<Rectangle> animationTargets = new List<Rectangle>(); 

            foreach (var point in invalidPoints)
            {
                Rectangle rect = null;

                if (_gameBoardArray[(int)point.X, (int)point.Y] != null)
                {
                    rect = _gameBoardArray[(int)point.X, (int)point.Y].Children[0] as Rectangle;
                }

                if ((rect != null) && (!animationTargets.Contains(rect)))
                {
                    //
                    // Make sure to not add the same target twice, or else the animation will throw
                    // an exception.
                    //

                    animationTargets.Add(rect);

                    var animation = CreateInvalidWordColorAnimation();

                    Storyboard.SetTarget(animation, rect);

                    Storyboard.SetTargetProperty(animation, "(Rectangle.Fill).(SolidColorBrush.Color)");

                    fadeIncorrectWordsToRed.Children.Add(animation);

                    _dirtyTiles.Add(rect);
                }
            }

            fadeIncorrectWordsToRed.Begin();
        }


        private ColorAnimation CreateInvalidWordColorAnimation()
        {
            var animation = new ColorAnimation
            {
                AutoReverse = false,
                EnableDependentAnimation = true,
                Duration = TimeSpan.FromMilliseconds(1000),
                To = (Colors.Red)
            };

            return animation;
        }


        private void OnGameStartedEvent(object sender)
        {
            GenerateBoxes();
            //ColorCircle.Begin();

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
            //GameBoardScrollViewer.Background = new SolidColorBrush(Colors.RoyalBlue) {Opacity = 0.1f};

            DumpCircle.Fill.Opacity = .7f;
            //TileRackUi.Background = new SolidColorBrush(Colors.MediumVioletRed);
            //TileRackUi.Background.Opacity = .4f;

            //TilePoolCountUI.SetBinding(TextBlock.TextProperty, 

            //Storyboard.SetTarget(ColorCircle, DumpCircle);
        }


        /// <summary>
        /// This method gets the index of the target container from the left hand column player panel.
        /// The index of the target container corresponding to the passed in user name is returned, on which
        /// animations are targeted for peel and dump actions.
        /// </summary>
        /// <param name="userNamae"></param>
        /// <returns></returns>
        private int GetPlayerPanelIndex(string userName)
        {
            //
            // Find the right item template instance from the string of the name of the Action Sender.
            //

            int index = 0;
            PlayerPaneViewModel curViewModel = null;
            foreach (PlayerPaneViewModel viewModel in _playerPane.PlayerList)
            {
                if (viewModel.PlayerName == userName)
                {
                    curViewModel = viewModel;
                    break;
                }

                index++;
            }

            if (null == curViewModel)
            {
                return -1;
            }

            return index;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">index of child element on which to perform the animation.</param>
        private void PlayCompletePeelAnimation(string actionSender)
        {
            var index = GetPlayerPanelIndex(actionSender);

            if (index == -1)
            {
                return;
            }

            var animationTargetContainer = PlayerPanel.ItemContainerGenerator.ContainerFromIndex(index);

            var grid = VisualTreeHelper.GetChild(animationTargetContainer, 0);
            var ellipse = VisualTreeHelper.GetChild(grid, 0);

            var colorAnimation = new ColorAnimationUsingKeyFrames();

            var frameA = new EasingColorKeyFrame();
            frameA.KeyTime = TimeSpan.FromMilliseconds(200);
            frameA.Value = Colors.Green;
            frameA.EasingFunction = new QuadraticEase();
            frameA.EasingFunction.EasingMode = EasingMode.EaseIn;

            var frameB = new EasingColorKeyFrame();
            frameB.KeyTime = TimeSpan.FromMilliseconds(400);
            frameB.Value = Colors.Orange;
            frameB.EasingFunction = new QuadraticEase();
            frameB.EasingFunction.EasingMode = EasingMode.EaseOut;

            colorAnimation.KeyFrames.Add(frameA);
            colorAnimation.KeyFrames.Add(frameB);

            Storyboard MyStoryBoard = new Storyboard();
            Storyboard.SetTargetProperty(colorAnimation, "(Ellipse.Stroke).(SolidColorBrush.Color)");
            Storyboard.SetTarget(colorAnimation, ellipse);

            MyStoryBoard.Children.Add(colorAnimation);
            MyStoryBoard.Begin();
            PlayPeelAnimation();
        }

        private void PlayDumpAnimation(string actionSender)
        {
            var index = GetPlayerPanelIndex(actionSender);

            if (index == -1)
            {
#if DEBUG
                throw new ArgumentException("Invalid index for the dump animation.");
#else
                index = 0;
#endif
            }

            var storyboard = CreateDumpAnimationStoryboard(index);

            storyboard.Begin();
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

            // ToDo: Update code below; UI has changed.

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
            boxOne.DoubleTapped += boxOne_DoubleTapped;
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
                BorderBrush = new SolidColorBrush(Colors.WhiteSmoke)

            };

            border.BorderBrush.Opacity = .4f;

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


        #region Dump and Peel Tile Animations

        private enum TileAnimationDirection
        {
            Up,
            Down
        }


        /// <summary>
        /// Scale X Animation Keyframes for tile animations.
        /// </summary>
        /// <returns></returns>
        private DoubleAnimationUsingKeyFrames CreateScaleXKeyFrames(int delay = 0)
        {
            var animation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");

            var frame = new EasingDoubleKeyFrame();
            frame.KeyTime = new KeyTime();
            frame.KeyTime = TimeSpan.FromMilliseconds(0);
            frame.Value = 0;

            var frameTwo = new EasingDoubleKeyFrame();
            frameTwo.KeyTime = new KeyTime();
            frameTwo.KeyTime = TimeSpan.FromMilliseconds(0 + delay);
            frameTwo.Value = 0;

            var frameThree = new EasingDoubleKeyFrame();
            frameThree.KeyTime = new KeyTime();
            frameThree.KeyTime = TimeSpan.FromMilliseconds(100 + delay);
            frameThree.Value = 1;

            //var frameThree = new EasingDoubleKeyFrame();
            //frameThree.KeyTime = new KeyTime();
            //frameThree.KeyTime = TimeSpan.FromMilliseconds(1000 + delay);
            //frameThree.Value = 1;

            animation.KeyFrames.Add(frame);
            animation.KeyFrames.Add(frameTwo);
            animation.KeyFrames.Add(frameThree);

            return animation;
        }


        private static DoubleAnimationUsingKeyFrames CreateScaleYKeyFrames(int delay = 0)
        {
            var animation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

            var frameA = new EasingDoubleKeyFrame();
            frameA.KeyTime = new KeyTime();
            frameA.KeyTime = TimeSpan.FromMilliseconds(0);
            frameA.Value = 0;

            var frameB = new EasingDoubleKeyFrame();
            frameB.KeyTime = new KeyTime();
            frameB.KeyTime = TimeSpan.FromMilliseconds(0 + delay);
            frameB.Value = 0;

            var frameC = new EasingDoubleKeyFrame();
            frameC.KeyTime = new KeyTime();
            frameC.KeyTime = TimeSpan.FromMilliseconds(100 + delay);
            frameC.Value = 1;

            animation.KeyFrames.Add(frameA);
            animation.KeyFrames.Add(frameB);
            animation.KeyFrames.Add(frameC);

            return animation;
        }


        private DoubleAnimationUsingKeyFrames CreateOpacityKeyFrames(int delay = 0)
        {
            var opacityAnimation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(opacityAnimation, "(UIElement.Opacity)");

            var oFrameZero = new EasingDoubleKeyFrame();
            oFrameZero.KeyTime = new KeyTime();
            oFrameZero.KeyTime = TimeSpan.FromMilliseconds(0);
            oFrameZero.Value = 1;

            var oFrameOne = new EasingDoubleKeyFrame();
            oFrameOne.KeyTime = new KeyTime();
            oFrameOne.KeyTime = TimeSpan.FromMilliseconds(500 + delay);
            oFrameOne.Value = 1;


            var oFrameTwo = new EasingDoubleKeyFrame();
            oFrameTwo.KeyTime = new KeyTime();
            oFrameTwo.KeyTime = TimeSpan.FromMilliseconds(2000 + delay);
            oFrameTwo.Value = 0;

            oFrameTwo.EasingFunction = new QuinticEase();
            oFrameTwo.EasingFunction.EasingMode = EasingMode.EaseOut;

            opacityAnimation.KeyFrames.Add(oFrameZero);
            opacityAnimation.KeyFrames.Add(oFrameOne);
            opacityAnimation.KeyFrames.Add(oFrameTwo);

            return opacityAnimation;
        }


        private DoubleAnimationUsingKeyFrames CreateTranslationKeyFrames(int playerViewModelIndex, TileAnimationDirection direction, Grid tile, int delay = 0)
        {
            Point BeginPoint;
            Point EndPoint;

            var animation = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            var dumpCircleGt = InnerDumpCircle.TransformToVisual(pageRoot);

            var animationTargetContainer = PlayerPanel.ItemContainerGenerator.ContainerFromIndex(playerViewModelIndex);

            var grid = VisualTreeHelper.GetChild(animationTargetContainer, 0) as Grid;

            var playerViewModelGt = grid.TransformToVisual(pageRoot);

            if (direction == TileAnimationDirection.Down)
            {
                BeginPoint = dumpCircleGt.TransformPoint(new Point(0, 0));

                EndPoint = playerViewModelGt.TransformPoint(new Point(0, 0));

                tile.Margin = new Thickness(BeginPoint.X + InnerDumpCircle.Width / 2 - tile.Width / 2, BeginPoint.Y - tile.Height / 2 + 10/* + InnerDumpCircle.Height / 2*/, 0, 0);
            }
            else
            {
                EndPoint = dumpCircleGt.TransformPoint(new Point(0, 0));

                BeginPoint = playerViewModelGt.TransformPoint(new Point(0, 0));

                tile.Margin = new Thickness(BeginPoint.X + InnerDumpCircle.Width / 2 + 15, tile.Height / 2 + 10, 0, 0);
            }

            var acFrame = new EasingDoubleKeyFrame();
            acFrame.KeyTime = new KeyTime();
            acFrame.KeyTime = TimeSpan.FromMilliseconds(0);
            acFrame.Value = BeginPoint.Y;

            var bcFrame = new EasingDoubleKeyFrame();
            bcFrame.KeyTime = new KeyTime();
            bcFrame.KeyTime = TimeSpan.FromMilliseconds(0 + delay);
            bcFrame.Value = BeginPoint.Y;


            var cFrame = new EasingDoubleKeyFrame();
            cFrame.KeyTime = new KeyTime();
            cFrame.KeyTime = TimeSpan.FromMilliseconds(100 + delay);
            cFrame.Value = BeginPoint.Y;

            var dFrame = new EasingDoubleKeyFrame();
            dFrame.KeyTime = new KeyTime();
            dFrame.KeyTime = TimeSpan.FromMilliseconds(500 + delay);
            dFrame.Value = EndPoint.Y;

            dFrame.EasingFunction = new QuadraticEase();
            dFrame.EasingFunction.EasingMode = EasingMode.EaseInOut;

            animation.KeyFrames.Add(acFrame);
            animation.KeyFrames.Add(bcFrame);
            animation.KeyFrames.Add(cFrame);
            animation.KeyFrames.Add(dFrame);

            return animation;
        }


        private Storyboard CreatePeelAnimationStoryboard(int playerViewModelIndex, TileAnimationDirection direction, out Grid tile)
        {
            //
            // Initialize the tile grid.
            //

            tile = new Grid()
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Colors.DodgerBlue)
            };

            tile.VerticalAlignment = VerticalAlignment.Top;
            tile.HorizontalAlignment = HorizontalAlignment.Left;
            tile.SetValue(Grid.ColumnProperty, 0);

            RootGrid.Children.Add(tile);

            tile.RenderTransformOrigin = new Point(.5, .5);
            tile.RenderTransform = new CompositeTransform();


            var storyboard = new Storyboard();

            storyboard.Children.Add(CreateScaleXKeyFrames());

            storyboard.Children.Add(CreateScaleYKeyFrames());

            storyboard.Children.Add(CreateTranslationKeyFrames(playerViewModelIndex, direction, tile));

            storyboard.Children.Add(CreateOpacityKeyFrames());

            return storyboard;
        }

        private Grid CreateAnimationTile()
        {
            var tile = new Grid()
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Colors.DodgerBlue)
            };

            tile.VerticalAlignment = VerticalAlignment.Top;
            tile.HorizontalAlignment = HorizontalAlignment.Left;
            tile.SetValue(Grid.ColumnProperty, 0);

            tile.RenderTransformOrigin = new Point(.5, .5);
            tile.RenderTransform = new CompositeTransform();

            return tile;
        }

        private Storyboard CreateDumpAnimationStoryboard(int playerViewModelIndex)
        {
            Storyboard storyboard = new Storyboard();

            var tileOne = CreateAnimationTile();
            var tileTwo = CreateAnimationTile();
            var tileThree = CreateAnimationTile();
            var tileFour = CreateAnimationTile();

            RootGrid.Children.Add(tileOne);
            RootGrid.Children.Add(tileTwo);
            RootGrid.Children.Add(tileThree);
            RootGrid.Children.Add(tileFour);

            var tiles = new List<Grid>() { tileOne, tileTwo, tileThree, tileFour };

            int i = 0;
            int delay = 0;

            foreach (var tile in tiles)
            {
                if (i == 1)
                {
                    delay += 500;
                }
                else if (i > 1)
                {
                    delay += 200;
                }

                var upAnimationScaleX = CreateScaleXKeyFrames(delay);
                Storyboard.SetTarget(upAnimationScaleX, tile);
                storyboard.Children.Add(upAnimationScaleX);

                var upAnimationScaleY = CreateScaleYKeyFrames(delay);
                Storyboard.SetTarget(upAnimationScaleY, tile);
                storyboard.Children.Add(upAnimationScaleY);

                var upAnimationOpacity = CreateOpacityKeyFrames(delay);
                Storyboard.SetTarget(upAnimationOpacity, tile);
                storyboard.Children.Add(upAnimationOpacity);

                i++;
            }


            var upAnimationTranslate = CreateTranslationKeyFrames(playerViewModelIndex, TileAnimationDirection.Up, tileOne);
            Storyboard.SetTarget(upAnimationTranslate, tileOne);
            storyboard.Children.Add(upAnimationTranslate);

            var downAnimationTranslateOne = CreateTranslationKeyFrames(playerViewModelIndex, TileAnimationDirection.Down, tileTwo, 500);
            Storyboard.SetTarget(downAnimationTranslateOne, tileTwo);

            var downAnimationTranslateTwo = CreateTranslationKeyFrames(playerViewModelIndex, TileAnimationDirection.Down, tileThree, 700);
            Storyboard.SetTarget(downAnimationTranslateTwo, tileThree);

            var downAnimationTranslateThree = CreateTranslationKeyFrames(playerViewModelIndex, TileAnimationDirection.Down, tileFour, 900);
            Storyboard.SetTarget(downAnimationTranslateThree, tileFour);

            storyboard.Children.Add(downAnimationTranslateOne);
            storyboard.Children.Add(downAnimationTranslateTwo);
            storyboard.Children.Add(downAnimationTranslateThree);

            return storyboard;
        }

        #endregion


        #region Box Manipulation Handlers



        /// <summary>
        /// Maps to a dump now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void boxOne_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var gameController = GameController.GetInstance();

            var box = sender as Grid;

            if (box == null)
            {
                return;
            }
            var textBlock = box.Children[1] as TextBlock;

            var returnedTile = new Tile(textBlock.Text);

            //ToDo: The line below is super hacky...yuck yuck.  Should not use app.clientbuddyinstance here, but instead use the performdumpaction method of humanplayer.cs
            var tiles = await gameController.PerformDumpAction(Settings.Alias, returnedTile);


            //
            // If there aren't enough tiles left for a dump, then return the tile back to the 
            // rack.
            //

            if (tiles.Count == 0)
            {
                return;
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

                //ToDo: Figure out how to move this shit back to XAML...God damn it XAML.
                var scaleXAnimation = new DoubleAnimation();
                scaleXAnimation.Duration = TimeSpan.FromMilliseconds(500);
                scaleXAnimation.To = 0;
                Storyboard.SetTargetProperty(scaleXAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");

                var scaleYAnimation = new DoubleAnimation();
                scaleYAnimation.Duration = TimeSpan.FromMilliseconds(500);
                scaleYAnimation.To = 0;
                Storyboard.SetTargetProperty(scaleYAnimation, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

                var rotationAnimation = new DoubleAnimation();
                rotationAnimation.Duration = TimeSpan.FromMilliseconds(500);
                rotationAnimation.To = 720;
                Storyboard.SetTargetProperty(rotationAnimation, "(UIElement.RenderTransform).(CompositeTransform.Rotation)");

                var TileVanishStoryboard = new Storyboard();

                TileVanishStoryboard.Children.Add(scaleXAnimation);
                TileVanishStoryboard.Children.Add(scaleYAnimation);
                TileVanishStoryboard.Children.Add(rotationAnimation);


                box.RenderTransform = new CompositeTransform();
                box.RenderTransformOrigin = new Point(.5, .5);

                //Storyboard.SetTarget(TileVanishStoryboard, box);
                //TileVanishStoryboard.Begin();
                //TileVanishStoryboard.Completed += TileSpinAndVanish_Completed;

                TileSpinAndVanish.Stop();
                Storyboard.SetTarget(TileSpinAndVanish, box);
                TileSpinAndVanish.Begin();
                PlayDumpAnimation(Settings.Alias);

                TileSpinAndVanish.Completed += TileSpinAndVanish_Completed;

                // ToDo: Remove this sleep and use the TileSpinAndVanishCompleted EH.
                await Task.Delay(500);


                //
                // Clear the game board spot from where the tile was dumped
                //

                var curColumn = (int)box.GetValue(Grid.ColumnProperty);
                var curRow = (int)box.GetValue(Grid.RowProperty);

                gameController = GameController.GetInstance();
                gameController.ClearBoardSpace(_westLimit + curColumn, _northLimit + curRow);

                
                box.Visibility = Visibility.Collapsed;
                GameBoard.Children.Remove(box);

                //_playedTiles.Remove(box); huh?  what's the purpose of this...


            }
        }




        private void BoxOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var box = sender as Grid;

            if (box == null)
            {
                return;
            }


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


            //
            // Restrict the tile movement to only be within the scroll viewer.
            //

            var gt = dragableItem.TransformToVisual(pageRoot);
            var endOfDragPosition = gt.TransformPoint(new Point(0, 0));

            gt = GameBoardScrollViewer.TransformToVisual(pageRoot);
            var scrollViewerAnchorPoint = gt.TransformPoint(new Point(0, 0));

            if ((endOfDragPosition.X < scrollViewerAnchorPoint.X) ||
                ((endOfDragPosition.X > scrollViewerAnchorPoint.X + GameBoardScrollViewer.ViewportWidth - dragableItem.Width * intendedZoomFactor) && (dragableItem.Parent != TileRackUi)))
            {
                translateTransform.X -= (e.Delta.Translation.X * (1 / intendedZoomFactor));
                translateTransform.Y -= (e.Delta.Translation.Y * (1 / intendedZoomFactor));
            }

            e.Handled = true;
        }


        private void Box_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            // ToDo: Define these magic numbers. Look at desiredDeceleration property.
            e.TranslationBehavior.DesiredDeceleration = 500.0 * 96.0 / (1000.0 * 1000.0);
            e.Handled = true;
        }


        private void BoxOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var gameController = GameController.GetInstance();

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


            ////
            //// Check if the user is trying to dump the tile into the tile pool:
            ////

            //else if ((endOfDragPosition.X > dumpBoxAnchorPoint.X) 
            //    && (dumpBoxAnchorPoint.X + DumpBox.ActualWidth > endOfDragPosition.X)
            //    && (endOfDragPosition.Y > dumpBoxAnchorPoint.Y)
            //    && (dumpBoxAnchorPoint.Y + DumpBox.ActualHeight > endOfDragPosition.Y))
            //{
            //    var returnedTile = new Tile(textBlock.Text);


            //    //ToDo: The line below is super hacky...yuck yuck.  Should not use app.clientbuddyinstance here, but instead use the performdumpaction method of humanplayer.cs
            //    var tiles = await gameController.PerformDumpAction(Settings.Alias, returnedTile);


            //    //
            //    // If there aren't enough tiles left for a dump, then return the tile back to the 
            //    // rack.
            //    //

            //    if (tiles.Count == 0)
            //    {
            //        //ToDo: This code was copied from the above "user has put the tile back on the tile rack section.  Consolidate this.
            //        if (box.Parent == TileRackUi)
            //        {
            //            _tileRack.RemoveTile(box);
            //            gameController.RemoveTileFromRack(textBlock.Text);
            //        }

            //        if (translateTransform != null)
            //        {
            //            translateTransform.X = 0;
            //            translateTransform.Y = 0;
            //        }

            //        _tileRack.AddTile(box);
            //        gameController.ReturnTileToRack(textBlock.Text);
            //    }

            //    else
            //    {
            //        if (box.Parent == TileRackUi)
            //        {
            //            _tileRack.RemoveTile(box);
            //            gameController.RemoveTileFromRack(textBlock.Text);
            //        }

            //        foreach (var tile in tiles)
            //        {
            //            var newTile = CreateLetterTile(tile.TileContents);
            //            _tileRack.AddTile(newTile);
            //            gameController.ReturnTileToRack(tile.TileContents);
            //        }

            //        box.RenderTransform = new CompositeTransform();
            //        Storyboard.SetTarget(TileSpinAndVanish, box);
            //        TileSpinAndVanish.Completed += TileSpinAndVanish_Completed;
            //    }
            //}


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


        void TileSpinAndVanish_Completed(object sender, object e)
        {
            //ToDo: Cleanup the objects.
            //box.Visibility = Visibility.Collapsed;
        }


        private void GameBoardScrollViewer_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var zoomPoint = e.GetPosition(GameBoardScrollViewer);
            GameBoardScrollViewer.ScrollToHorizontalOffset(zoomPoint.X);
            GameBoardScrollViewer.ScrollToVerticalOffset(zoomPoint.Y);

            GameBoardScrollViewer.ZoomToFactor(GameBoardScrollViewer.ZoomFactor > 1.5f ? .5f : 2.0f);
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
            else
            {
                var serverProxy = ServerProxy.GetInstance();

                if (serverProxy.messageSender != null)
                {
                    await serverProxy.messageSender.SendMessage(PacketType.ClientReadyForGameStart);
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



        private void PlayPeelAnimation()
        {
            List<Storyboard> storyboards = new List<Storyboard>();

            for (var i = 0; i < PlayerPanel.Items.Count; i++)
            {
                Grid newTile = null;
                var storyboard = CreatePeelAnimationStoryboard(i, TileAnimationDirection.Down, out newTile);
                Storyboard.SetTarget(storyboard, newTile);
                storyboards.Add(storyboard);
            }

            foreach(var s in storyboards)
            {
                s.Begin();
            }
        }


        private async void PlayDumpAnimation(int playerViewModelIndex)
        {
            var storyboard = CreateDumpAnimationStoryboard(playerViewModelIndex);
            storyboard.Begin();

            //const int TilesPerDump = 3;

            //List<Storyboard> storyboards = new List<Storyboard>();

            //Grid newTile = null;
            //var upStoryboard = CreatePeelAnimationStoryboard(playerViewModelIndex, TileAnimationDirection.Up, out newTile);
            //Storyboard.SetTarget(upStoryboard, newTile);



            //upStoryboard.Begin();

            //await Task.Delay(600);

            //for (var i = 0; i < TilesPerDump; i++)
            //{
            //    Grid newIncomingTile = null;
            //    var storyboard = CreatePeelAnimationStoryboard(playerViewModelIndex, TileAnimationDirection.Down, out newIncomingTile);
            //    Storyboard.SetTarget(storyboard, newTile);
            //    storyboards.Add(storyboard);
            //}

            //foreach (var s in storyboards)
            //{
            //    s.Begin();
            //    await Task.Delay(500);
            //}
        }


        private void PeelButton_Click(object sender, RoutedEventArgs e)
        {
            //PlayPeelAnimation();
            PlayCompletePeelAnimation("Jonathan");
        }

        private void DumpButton_Click(object sender, RoutedEventArgs e)
        {
            PlayDumpAnimation(0);
        }

    }
}
