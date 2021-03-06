﻿using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ozwego.Converters
{
    public class ConnectionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = value ?? true;
            bool truth;
            Boolean.TryParse(val.ToString(), out truth);
            return truth ? "connected to ozwego.net"
                : "offline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
