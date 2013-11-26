using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Ozwego.UI.Background
{
    public enum ColorScheme
    {
        Purple,
        Red,
        Green
    }



    public static class ColorGenerator
    {
        private static readonly Random Rand = new Random();

        private static readonly List<SolidColorBrush> PurpleScheme = new List<SolidColorBrush>()
            {
                new SolidColorBrush(Color.FromArgb(255, 21, 143, 141)),
                new SolidColorBrush(Color.FromArgb(255, 141, 21, 143)),
                new SolidColorBrush(Color.FromArgb(255, 21, 84, 143)),
                new SolidColorBrush(Color.FromArgb(255, 80, 21, 143)),
                new SolidColorBrush(Color.FromArgb(255, 21, 23, 143))
            };

        private static readonly List<SolidColorBrush> RedScheme = new List<SolidColorBrush>()
            {
                new SolidColorBrush(Color.FromArgb(255, 230, 0, 0)),
                new SolidColorBrush(Color.FromArgb(255, 230, 100, 0)),
                new SolidColorBrush(Color.FromArgb(255, 230, 215, 0)),
                new SolidColorBrush(Color.FromArgb(255, 230, 0, 130)),
                new SolidColorBrush(Color.FromArgb(255, 215, 0, 230))
            };

        private static readonly List<SolidColorBrush> GreenScheme = new List<SolidColorBrush>()
            {
                new SolidColorBrush(Color.FromArgb(255, 0, 120, 48)),
                new SolidColorBrush(Color.FromArgb(255, 0, 120, 108)),
                new SolidColorBrush(Color.FromArgb(255, 0, 72, 120)),
                new SolidColorBrush(Color.FromArgb(255, 12, 120, 0)),
                new SolidColorBrush(Color.FromArgb(255, 72, 120, 0))
            };


        public static List<SolidColorBrush> GetColors(ColorScheme scheme)
        {
            var colorList = new List<SolidColorBrush>();

            switch (scheme)
            {
                case ColorScheme.Purple:
                    colorList = PurpleScheme;
                    break;

                case ColorScheme.Red:
                    colorList = RedScheme;
                    break;

                case ColorScheme.Green:
                    colorList = GreenScheme;
                    break;
            }

            return colorList;
        }


        public static SolidColorBrush GetRandomColor(ColorScheme scheme)
        {
            SolidColorBrush retVal = null;

            switch (scheme)
            {
                case ColorScheme.Purple:
                    retVal = RandomColorFromList(PurpleScheme);
                    break;

                case ColorScheme.Red:
                    retVal = RandomColorFromList(RedScheme);
                    break;

                case ColorScheme.Green:
                    retVal = RandomColorFromList(GreenScheme);
                    break;
            }

            return retVal;
        }


        private static SolidColorBrush RandomColorFromList(List<SolidColorBrush> colorList)
        {
            var count = colorList.Count;
            return colorList.ElementAt(Rand.Next(0, count));
        }
    }
}
