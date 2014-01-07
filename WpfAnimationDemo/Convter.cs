using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace WpfAnimationDemo
{
    public class Convter : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            AnimationInfo.CheckBoxStates state = (AnimationInfo.CheckBoxStates)value;
            switch (state)
            {
                case AnimationInfo.CheckBoxStates.Pause:
                    return App.Current.FindResource("CheckBoxStyleAnimation") as Style;
                case AnimationInfo.CheckBoxStates.Resume:
                    return App.Current.FindResource("CheckBoxStyleAnimation") as Style;
                case AnimationInfo.CheckBoxStates.Stop:
                    return App.Current.FindResource("CheckBoxStyleAnimationFinished") as Style;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
