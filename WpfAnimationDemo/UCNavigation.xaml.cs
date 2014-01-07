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
    /// UCNavigation.xaml 的交互逻辑
    /// </summary>
    public partial class UCNavigation : UserControl
    {
        #region 变量
        string filePath = AppDomain.CurrentDomain.BaseDirectory + @"map.jpg";
        double zoomScale = 1.1;
        double left;
        double top;
        double leftChanged;
        double topChanged;
        Point mousePostion;
        Point CanvasMainLocation;
        Point targetPoint;
        bool isShow = true;
        #endregion

        #region 委托
        public delegate void CenterLocationChangedHandle(Point nowCenterPoint);
        public event CenterLocationChangedHandle CenterLocationChanged;
        public void OnCenterLocationChanged(Point nowCenterPoint)
        {
            if (CenterLocationChanged != null)
            {
                CenterLocationChanged.Invoke(nowCenterPoint);
            }
        }
        #endregion

        #region 属性
        public double MapScaleWH { get; set; }
        public double ScaleUCMapToRealMap { get; set; }
        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public UCNavigation()
        {
            this.InitializeComponent();
        }
        #endregion

        #region 业务
        private void CanvasMain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            left = Canvas.GetLeft(rectNavi);
            top = Canvas.GetTop(rectNavi);
            double oldWidth = rectNavi.Width;
            double oldHeight = rectNavi.Height;
            //按比例缩放Rect

            if (e.Delta > 0)
            {
                if (oldWidth >= this.Width - 50)
                {
                    return;
                }
                rectNavi.Width = rectNavi.Width * zoomScale;
                rectNavi.Height = rectNavi.Height * zoomScale;
            }
            else
            {
                if (oldWidth <= 30)
                {
                    return;
                }
                rectNavi.Width = rectNavi.Width / zoomScale;
                rectNavi.Height = rectNavi.Height / zoomScale;
            }

            leftChanged = (oldWidth - rectNavi.Width) / 2;
            topChanged = (oldHeight - rectNavi.Height) / 2;
            Canvas.SetLeft(rectNavi, left + leftChanged);
            Canvas.SetTop(rectNavi, top + topChanged);

        }

        private void rectNavi_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var targetElement = e.Source as IInputElement;

            if (targetElement != null)
            {
                //targetPoint = e.GetPosition(targetElement);
                CanvasMainLocation.X = Canvas.GetLeft(rectNavi);
                CanvasMainLocation.Y = Canvas.GetTop(rectNavi);
                targetPoint = e.GetPosition(CanvasMain);
                //开始捕获鼠标
                targetElement.CaptureMouse();
            }
        }

        private void rectNavi_MouseMove(object sender, MouseEventArgs e)
        {
            //移动Rect
            //确定鼠标左键处于按下状态并且有元素被选中
            var targetElement = Mouse.Captured as UIElement;

            if (e.LeftButton == MouseButtonState.Pressed && targetElement != null)
            {
                var pCanvas = e.GetPosition(CanvasMain);
                //设置最终位置
                Canvas.SetLeft(rectNavi, pCanvas.X - targetPoint.X + CanvasMainLocation.X);
                Canvas.SetTop(rectNavi, pCanvas.Y - targetPoint.Y + CanvasMainLocation.Y);

                this.Cursor = System.Windows.Input.Cursors.ScrollAll;

                Point nowCenPt = new Point((Canvas.GetLeft(rectNavi) + rectNavi.Width / 2) / ScaleUCMapToRealMap, (Canvas.GetTop(rectNavi) + rectNavi.Height / 2) / ScaleUCMapToRealMap);
                OnCenterLocationChanged(nowCenPt);
            }
        }

        private void rectNavi_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ////取消捕获鼠标   
            Mouse.Capture(null);
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void CanvasMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePostion = e.GetPosition(this);
            left = mousePostion.X - rectNavi.Width / 2;
            top = mousePostion.Y - rectNavi.Height / 2;
            Canvas.SetLeft(rectNavi, left);
            Canvas.SetTop(rectNavi, top);

            Point nowCenPt = new Point((Canvas.GetLeft(rectNavi) + rectNavi.Width / 2) / ScaleUCMapToRealMap, (Canvas.GetTop(rectNavi) + rectNavi.Height / 2) / ScaleUCMapToRealMap);
            OnCenterLocationChanged(nowCenPt);
        }

        /// <summary>
        /// 加载底图
        /// </summary>
        /// <param name="path"></param>
        public void SetBackGroundImg()
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
            MapScaleWH = (double)img.Width / img.Height;

            this.Width = 220;
            this.Height = this.Width / MapScaleWH;

            rectNavi.Width = 50;
            rectNavi.Height = rectNavi.Width / MapScaleWH;

            Canvas.SetLeft(rectNavi, (this.Width - rectNavi.Width) / 2);
            Canvas.SetTop(rectNavi, (this.Height - rectNavi.Height) / 2);

            ImageBrush mapBrush = new ImageBrush();
            mapBrush.ImageSource = new BitmapImage(new Uri(filePath, UriKind.Relative));
            BorderMain.Background = mapBrush;

            ScaleUCMapToRealMap = this.Width / img.Width;
        }

        private void CanvasMain_MouseEnter(object sender, MouseEventArgs e)
        {
            btnShow.Visibility = System.Windows.Visibility.Visible;
        }

        private void CanvasMain_MouseLeave(object sender, MouseEventArgs e)
        {
            btnShow.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HideAnimation()
        {
            Duration dur = TimeSpan.FromSeconds(0.2);
            tt.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(this.Width + 4, dur));
            //st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0, dur));
        }

        private void ShowAnimation()
        {
            Duration dur = TimeSpan.FromSeconds(0.2);
            tt.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, dur));
            //st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, dur));
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            if (isShow)
            {
                HideAnimation();
                isShow = false;
            }
            else
            {
                ShowAnimation();
                isShow = true;
            }
        }
        #endregion
    }
}