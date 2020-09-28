using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Snikmorder.DesktopClient
{
    public static class VisibilityConverters
    {
        #region IsTrue

        public static IValueConverter IsTrue { get; } = new IsTrueValueConverter();

        private class IsTrueValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(value is bool boolean)
                    return boolean ? Visibility.Visible : Visibility.Collapsed;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion

        #region IsTrueOrHidden

        public static IValueConverter IsTrueOrHidden { get; } = new IsTrueOrHiddenValueConverter();

        private class IsTrueOrHiddenValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(value is bool boolean)
                    return boolean ? Visibility.Visible : Visibility.Hidden;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion

        #region IsFalse

        public static IValueConverter IsFalse { get; } = new IsFalseValueConverter();

        private class IsFalseValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(value is bool boolean)
                    return boolean ? Visibility.Collapsed : Visibility.Visible;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion

        #region IsFalse

        public static IValueConverter IsFalseOrHidden { get; } = new IsFalseOrHiddenValueConverter();

        private class IsFalseOrHiddenValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if(value is bool boolean)
                    return boolean ? Visibility.Hidden : Visibility.Visible;
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion

        #region IsNull

        public static IValueConverter IsNull { get; } = new IsNullValueConverter();

        private class IsNullValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is null ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion

        #region IsNotNull

        public static IValueConverter IsNotNull { get; } = new IsNotNullValueConverter();

        private class IsNotNullValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is null ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }

        #endregion
    }
}