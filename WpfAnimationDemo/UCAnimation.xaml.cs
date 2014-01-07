using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataAccess;
using System.Collections.ObjectModel;
using System.Data;

namespace WpfAnimationDemo
{
    /// <summary>
    /// UCAnimation.xaml 的交互逻辑
    /// </summary>
    public partial class UCAnimation : UserControl
    {
        #region 变量
        public AnimationBase ab;
        private Point mousePosition = new Point(0, 0);
        List<Point> ListPoint = new List<Point>();
        List<Station> ListStation = new List<Station>();
        List<Button> ListButton = new List<Button>();
        Point targetPoint;
        Point CanvasMainLocation;
        BLL bll = new BLL();
        Dictionary<string, Point> dictStationPoint = new Dictionary<string, Point>();
        double loadScale;//initlization map scale for carete mover path,because CanvasMain size will not change any time since Window loaded
        public double speed = 20;//speed value 20|50|100
        double mapWidth;//Map file's real width
        double mapHeight;//Map file's real height
        double biliUp = 1.2;//缩放增大比例
        double biliDown = 0.8;//缩放缩小比例
        public bool isStationFixSize = true;//是否固定分站大小
        #endregion

        #region 委托

        #endregion

        #region 属性
        public ObservableCollection<EmpMover> CollectionEmp { get; set; }
        public ObservableCollection<string> CollectionFiledMsg { get; set; }
        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public UCAnimation()
        {
            this.InitializeComponent();
            ab = new AnimationBase(this, CanvasMain, ViewBoxMain, CanvasRoot);
            AnimationBase.ListBoxMap = lbAnimaStatus;
            CollectionEmp = new ObservableCollection<EmpMover>();
            CollectionFiledMsg = new ObservableCollection<string>();
            lbAnimaStatus.ItemsSource = ab.CollectionAnmInfoSource;
            lbFiled.ItemsSource = CollectionFiledMsg;
        }
        #endregion

        #region 业务
        #region UI逻辑
        private void CanvasMain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePoint = e.GetPosition(ViewBoxMain);//获取当前的点
            Point NewPoint = new Point();
            Point locationPoint = new Point();         //控件左上角坐标 
            locationPoint.X = Canvas.GetLeft(ViewBoxMain);  //获得Map缩放前相对主静态容器的左上角坐标
            locationPoint.Y = Canvas.GetTop(ViewBoxMain);

            double width = ViewBoxMain.Width;
            double height = ViewBoxMain.Height;

            //设置缩放箭头位置
            Point narrowPoint = e.GetPosition(CanvasRoot);//获取鼠标在CanvasRoot当前的点
            double narrowLeft = narrowPoint.X - ucDirectionArrow.Width / 2;
            double narrowTop = narrowPoint.Y - ucDirectionArrow.Height / 2;
            Canvas.SetLeft(ucDirectionArrow, narrowLeft);
            Canvas.SetTop(ucDirectionArrow, narrowTop);

            if (e.Delta > 0)//放大
            {
                if (width >= 5000)
                {
                    return;
                }
                ViewBoxMain.Width = width * biliUp;
                ViewBoxMain.Height = height * biliUp;

                NewPoint.X = locationPoint.X - mousePoint.X * (biliUp - 1);
                NewPoint.Y = locationPoint.Y - mousePoint.Y * (biliUp - 1);

                if (isStationFixSize)
                {
                    //还原分站控件的缩放
                    foreach (Button item in ListButton)
                    {
                        ScaleTransform st = item.RenderTransform as ScaleTransform;
                        st.ScaleX = st.ScaleX / biliUp;
                        st.ScaleY = st.ScaleY / biliUp;
                    }
                    //还原动画控件的缩放
                    foreach (Button item in ab.ListButton)
                    {
                        ScaleTransform st = ((TransformGroup)item.RenderTransform).Children[0] as ScaleTransform;
                        st.ScaleX = st.ScaleX / biliUp;
                        st.ScaleY = st.ScaleY / biliUp;
                    }
                }
                else
                {
                    foreach (Button item in ListButton)
                    {
                        ScaleTransform st = item.RenderTransform as ScaleTransform;
                        st.ScaleX = 1.0;
                        st.ScaleY = 1.0;
                    }

                    foreach (Button item in ab.ListButton)
                    {
                        ScaleTransform st = ((TransformGroup)item.RenderTransform).Children[0] as ScaleTransform;
                        st.ScaleX = 1.0;
                        st.ScaleY = 1.0;
                    }
                }

                //放大箭头动画
                ucDirectionArrow.ZoomOut();
            }
            else
            {
                if (width <= 300)
                {
                    return;
                }
                ViewBoxMain.Width = width * biliDown;
                ViewBoxMain.Height = height * biliDown;

                NewPoint.X = locationPoint.X + mousePoint.X * (1 - biliDown);
                NewPoint.Y = locationPoint.Y + mousePoint.Y * (1 - biliDown);

                if (isStationFixSize)
                {
                    //还原分站的缩放
                    foreach (Button item in ListButton)
                    {
                        ScaleTransform st = item.RenderTransform as ScaleTransform;
                        st.ScaleX = st.ScaleX / biliDown;
                        st.ScaleY = st.ScaleY / biliDown;
                    }
                    //还原动画控件的缩放
                    foreach (Button item in ab.ListButton)
                    {
                        ScaleTransform st = ((TransformGroup)item.RenderTransform).Children[0] as ScaleTransform;
                        st.ScaleX = st.ScaleX / biliDown;
                        st.ScaleY = st.ScaleY / biliDown;
                    }
                }
                else
                {
                    foreach (Button item in ListButton)
                    {
                        ScaleTransform st = item.RenderTransform as ScaleTransform;
                        st.ScaleX = 1.0;
                        st.ScaleY = 1.0;
                    }

                    foreach (Button item in ab.ListButton)
                    {
                        ScaleTransform st = ((TransformGroup)item.RenderTransform).Children[0] as ScaleTransform;
                        st.ScaleX = 1.0;
                        st.ScaleY = 1.0;
                    }
                }

                //缩小箭头动画
                ucDirectionArrow.ZoomIn();
            }

            Canvas.SetLeft(ViewBoxMain, NewPoint.X);
            Canvas.SetTop(ViewBoxMain, NewPoint.Y);

            e.Handled = true;
        }

        private void CanvasMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var targetElement = e.Source as IInputElement;

            if (targetElement != null)
            {
                CanvasMainLocation.X = Canvas.GetLeft(ViewBoxMain);
                CanvasMainLocation.Y = Canvas.GetTop(ViewBoxMain);
                targetPoint = e.GetPosition(CanvasRoot);
                //开始捕获鼠标
                targetElement.CaptureMouse();
            }
        }

        private void CanvasMain_MouseMove(object sender, MouseEventArgs e)
        {
            //确定鼠标左键处于按下状态并且有元素被选中
            var targetElement = Mouse.Captured as UIElement;

            if (e.LeftButton == MouseButtonState.Pressed && targetElement != null)
            {
                var pCanvas = e.GetPosition(CanvasRoot);
                //设置最终位置
                Canvas.SetLeft(ViewBoxMain, pCanvas.X - targetPoint.X + CanvasMainLocation.X);
                Canvas.SetTop(ViewBoxMain, pCanvas.Y - targetPoint.Y + CanvasMainLocation.Y);

                this.Cursor = System.Windows.Input.Cursors.ScrollAll;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ////取消捕获鼠标   
            Mouse.Capture(null);

            this.Cursor = System.Windows.Input.Cursors.Arrow;

            mousePosition = e.GetPosition(CanvasMain);
        }

        private void lbAnimaStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((AnimationInfo)e.RemovedItems[0]).IsSelected = false;
            }
            catch
            {
            }
            try
            {
                ((AnimationInfo)e.AddedItems[0]).IsSelected = true;
            }
            catch
            {
            }

        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Load()
        {
            LoadMap();

            double bili = mapWidth / mapHeight;//地图宽高比
            ViewBoxMain.Width = 1000;//初始宽
            ViewBoxMain.Height = ViewBoxMain.Width / bili;

            CanvasMain.Width = ViewBoxMain.Width;
            CanvasMain.Height = ViewBoxMain.Height;

            double widthCanvasRoot = CanvasRoot.ActualWidth;
            double heightCanvasRoot = CanvasRoot.ActualHeight;

            double widthViewBoxMain = ViewBoxMain.Width;
            double heightViewBoxMain = ViewBoxMain.Height;

            //设置居中
            double left = (widthCanvasRoot - widthViewBoxMain) / 2;
            double top = (heightCanvasRoot - heightViewBoxMain) / 2;
            Canvas.SetLeft(ViewBoxMain, left);
            Canvas.SetTop(ViewBoxMain, top);

            LoadStations();

            loadScale = ViewBoxMain.Width / mapWidth;

            ucNavigation.SetBackGroundImg();
            ucNavigation.CenterLocationChanged += new UCNavigation.CenterLocationChangedHandle(ucNavigation_CenterLocationChanged);
            ucNavigation.btnShow.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// 鹰眼关联逻辑
        /// </summary>
        /// <param name="nowCenterPoint"></param>
        void ucNavigation_CenterLocationChanged(Point nowCenterPoint)
        {
            double scale = ViewBoxMain.Width / mapWidth;
            Point nowCenPt = new Point(nowCenterPoint.X * scale, nowCenterPoint.Y * scale);
            Point winCenPt = new Point(CanvasRoot.ActualWidth / 2, CanvasRoot.ActualHeight / 2);
            Canvas.SetLeft(ViewBoxMain, winCenPt.X - nowCenPt.X);
            Canvas.SetTop(ViewBoxMain, winCenPt.Y - nowCenPt.Y);
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        private void LoadMap()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"map.jpg";
            byte[] mapBytes = bll.GetMapBytes();
            Converter.CreateFile(filePath, mapBytes);
            System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);
            mapWidth = img.Width;
            mapHeight = img.Height;
            ImageBrush mapBrush = new ImageBrush();
            mapBrush.ImageSource = new BitmapImage(new Uri(filePath, UriKind.Relative));
            CanvasMain.Background = mapBrush;

            //System.IO.File.Delete(filePath);
        }

        /// <summary>
        /// 加载分站
        /// </summary>
        private void LoadStations()
        {
            ListStation = bll.GetStationList();
            double scale = ViewBoxMain.Width / mapWidth;

            foreach (Station item in ListStation)
            {
                Button btn = new Button();
                btn.Width = 14;
                btn.Height = 14;
                btn.Background = new SolidColorBrush(Colors.Blue);
                btn.Foreground = new SolidColorBrush(Colors.White);
                btn.FontSize = 6;
                btn.Content = item.StationID;
                btn.ToolTip = item.StationName;
                btn.Style = FindResource("ButtonStyleStationNew") as Style;
                CanvasMain.Children.Add(btn);
                float nowX = (float)(item.StationPoint.X * scale);
                float nowY = (float)(item.StationPoint.Y * scale);
                Canvas.SetLeft(btn, nowX - btn.Width / 2);
                Canvas.SetTop(btn, nowY - btn.Height / 2);
                Canvas.SetZIndex(btn, 1);
                item.StationNowPoint = new System.Drawing.PointF(nowX, nowY);
                dictStationPoint.Add(item.StationID, new Point(nowX, nowY));

                ListButton.Add(btn);

                ScaleTransform st = new ScaleTransform();
                btn.RenderTransform = st;
                st.CenterX = btn.Width / 2;
                st.CenterY = btn.Height / 2;
            }
        }

        #region 播放轨迹
        public void PlayListHisRoute(DateTime dateBegin, DateTime dateEnd)
        {
            if (CollectionEmp.Count == 0)
            {
                return;
            }

            for (int i = 0; i < CollectionEmp.Count; i++)
            {
                PlayHisInOutStationHead(CollectionEmp[i].Name, CollectionEmp[i].EmpID, dateBegin, dateEnd);
                if (i == CollectionEmp.Count - 1)//Last EmpMover
                {
                    ab.LoadAnimationInfoCollection();
                }
            }
        }

        /// <summary>
        /// 播放历史轨迹
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dateBegin"></param>
        /// <param name="dateEnd"></param>
        private void PlayHisInOutStationHead(string name, string id, DateTime dateBegin, DateTime dateEnd)
        {
            List<string> listRouteStr = GetRouteInfoByEmpID(id, dateBegin, dateEnd, int.Parse(bll.mapID));
            if (listRouteStr == null || listRouteStr.Count == 0)
            {
                CollectionFiledMsg.Add(string.Format("{0}没有轨迹可以生成！", name));
                return;
            }
            string[] routeStrs = listRouteStr[0].Split('|');
            ListPoint.Clear();
            for (int i = 0; i < routeStrs.Length; i++)
            {
                double x = double.Parse(routeStrs[i].Split(',')[0]) * loadScale;
                double y = double.Parse(routeStrs[i].Split(',')[1]) * loadScale;
                ListPoint.Add(new Point(x, y));
            }

            Button btnEmp = new Button();
            btnEmp.Width = 12;
            btnEmp.Height = 12;
            btnEmp.ToolTip = name;
            btnEmp.Foreground = new SolidColorBrush(Colors.White);
            btnEmp.FontSize = 6;
            btnEmp.Style = FindResource("ButtonStyleEmp") as Style;
            btnEmp.IsEnabled = false;
            CanvasMain.Children.Add(btnEmp);
            Canvas.SetZIndex(btnEmp, 2);
            Canvas.SetLeft(btnEmp, 0);
            Canvas.SetTop(btnEmp, 0);

            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            st.CenterX = btnEmp.Width / 2;
            st.CenterY = btnEmp.Height / 2;
            tg.Children.Add(st);
            btnEmp.RenderTransform = tg;

            Path pathTemp = ab.CreatePathByPointList(ListPoint);
            int timeSpan = ab.GetTimeSpendByPointList(ListPoint, speed);
            ab.StartAnimation(pathTemp, btnEmp, timeSpan);


        }

        /// <summary>
        /// 根据人员ID 起始时间和结束时间得到该人员的历史轨迹
        /// </summary>
        /// <param name="id">人员ID</param>
        /// <param name="startdate">起始时间</param>
        /// <param name="enddate">结束时间</param>
        /// <returns>信息表</returns>
        public List<string> GetRouteInfoByEmpID(string id, DateTime startDate, DateTime endDate, int fileid)
        {
            DataTable oldDt = bll.GetRouteByUserID(int.Parse(id), startDate, endDate, fileid);
            if (oldDt != null)
            {
                if (oldDt.Rows.Count > 0)
                {
                    string routepoint = string.Empty;
                    bool isdesc = false;
                    int LastIndex = 0;
                    for (int i = 1; i < oldDt.Rows.Count; i++)
                    {
                        string nowname = oldDt.Rows[i]["StationPlace"].ToString();
                        if (oldDt.Rows[LastIndex]["StationPlace"].ToString() != oldDt.Rows[i]["StationPlace"].ToString() && oldDt.Rows[i]["StationPlace"].ToString() != "")
                        {
                            string towid;
                            float xy1 = float.Parse(oldDt.Rows[LastIndex]["StationAddress"].ToString() + "." + oldDt.Rows[LastIndex]["StationHeadAddress"].ToString());
                            float xy2 = float.Parse(oldDt.Rows[i]["StationAddress"].ToString() + "." + oldDt.Rows[i]["StationHeadAddress"].ToString());
                            LastIndex = i;

                            if (xy1 > xy2)
                            {
                                towid = xy2.ToString("F1") + "," + xy1.ToString("F1");
                                isdesc = true;
                            }
                            else
                            {
                                towid = xy1.ToString("F1") + "," + xy2.ToString("F1");
                                isdesc = false;
                            }
                            DataTable dt = bll.GetRoutePointByID(towid, isdesc, fileid.ToString());
                            if (dt.Rows.Count != 0)
                            {
                                for (int j = 0; j < dt.Rows.Count; j++)
                                {
                                    routepoint = routepoint + dt.Rows[j]["x"].ToString() + "," + dt.Rows[j]["y"].ToString() + "|";
                                }
                            }
                            else
                            {
                                //Czlt-2011-03-08-没有路径时添加
                                string strPath = string.Empty;

                                if (isdesc)
                                {
                                    //strPath = bll.GetWayByStaIDT(xy2.ToString("F1"), xy1.ToString("F1"), fileid.ToString());
                                }
                                else
                                {
                                    //strPath = bll.GetWayByStaIDT(xy1.ToString("F1"), xy2.ToString("F1"), fileid.ToString());

                                }

                                if (strPath.Equals(""))
                                {
                                    //Czlt-2012-04-20 将有问题的路径记录下来
                                    //strMessage += "["+towid + "];";
                                    //throw new Exception("路径尚未配置,或者配置的路径不符合要求,请检查...");
                                }
                                else
                                {
                                    routepoint = routepoint + strPath;
                                }
                            }
                        }
                    }

                    List<string> list = new List<string>();
                    if (routepoint != "")
                    {
                        list.Add(routepoint.Remove(routepoint.Length - 1));
                    }
                    return list;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public void StopAllAnimation()
        {
            foreach (AnimationInfo item in ab.CollectionAnmInfo)
            {
                if (item.IsFollowing)
                {
                    item.IsFollowing = false;
                }
            }

            ab.StopAllAnimation();
            CollectionFiledMsg.Clear();
        }

        /// <summary>
        /// 暂停所有动画
        /// </summary>
        public void PauseAllAnimation()
        {
            ab.PauseAllAnimation();
        }

        /// <summary>
        /// 继续所有动画
        /// </summary>
        public void ResumeAllAnimation()
        {
            ab.ResumeAllAnimation();
        }
        #endregion
        #endregion
    }
}