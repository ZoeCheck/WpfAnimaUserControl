using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace WpfAnimationDemo
{
    /// <summary>
    /// UCDirectionArrow.xaml 的交互逻辑
    /// </summary>
    public partial class UCDirectionArrow : UserControl
    {
        Storyboard sbZoomIn;
        Storyboard sbZoomOut;

        public UCDirectionArrow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 缩小
        /// </summary>
        public void ZoomIn()
        {
            if (sbZoomIn != null)
            {
                sbZoomIn.Begin();
            }
        }

        /// <summary>
        /// 放大
        /// </summary>
        public void ZoomOut()
        {
            if (sbZoomOut != null)
            {
                sbZoomOut.Begin();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            sbZoomIn = this.Resources["SBZoomIn"] as Storyboard;
            sbZoomOut = this.Resources["SBZoomOut"] as Storyboard;
        }
    }
}