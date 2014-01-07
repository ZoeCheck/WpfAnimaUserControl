using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Timers;

namespace WpfAnimationDemo
{
    /// <summary>
    /// 全局动画监控类
    /// </summary>
    public class AnimationBase : DependencyObject
    {
        #region 变量
        int RenderFlag = 0;//RenderTransform注册名
        private List<Storyboard> ListStoryboard;//For Stop Storyboard
        public List<Button> ListButton;//For remove Button form Canvas children
        private List<Path> ListPath;//For remove path from Canvas children
        public ObservableCollection<AnimationInfo> CollectionAnmInfo;
        public ObservableCollection<AnimationInfo> CollectionAnmInfoSource;
        private Timer timerLoadAB = new Timer();
        private int loadAnInfoCount = 0;
        TimeSpan tsUnload = TimeSpan.FromSeconds(0.1);//Unload CollectionAnmInfoSource time span
        #endregion

        #region 委托
        public delegate void FinishedAnimationHandle(string msg);
        public event FinishedAnimationHandle FinishedAnimationEvent;
        public void OnFinishedAnimationEvent(string msg)
        {
            if (FinishedAnimationEvent != null)
            {
                FinishedAnimationEvent(msg);
            }
        }

        public delegate void LoadAnimFunc(AnimationInfo ai);
        #endregion

        #region 属性
        public static Canvas CanvasMap { get; set; }
        public static Canvas CanvasRootMap { get; set; }
        public static UserControl userControlMap { get; set; }
        public static Viewbox ViewBoxMap { get; set; }
        public static ListBox ListBoxMap { get; set; }

        public bool IsFinishedAll
        {
            get { return (bool)GetValue(IsFinishedAllProperty); }
            set { SetValue(IsFinishedAllProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFinishedAll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFinishedAllProperty =
            DependencyProperty.Register("IsFinishedAll", typeof(bool), typeof(AnimationBase), new UIPropertyMetadata(true));

        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public AnimationBase(UserControl userControl, Canvas canvas, Viewbox vbMain, Canvas canvasRoot)
        {
            userControlMap = userControl;
            CanvasMap = canvas;
            NameScope.SetNameScope(userControlMap, new NameScope());
            ListStoryboard = new List<Storyboard>();
            ListButton = new List<Button>();
            ListPath = new List<Path>();
            CollectionAnmInfo = new ObservableCollection<AnimationInfo>();
            CollectionAnmInfoSource = new ObservableCollection<AnimationInfo>();
            ViewBoxMap = vbMain;
            CanvasRootMap = canvasRoot;
            timerLoadAB.Elapsed += new ElapsedEventHandler(timerLoadAB_Elapsed);
            timerLoadAB.Interval = 150;
        }

        #endregion

        #region 业务
        #region 创建路径
        /// <summary>
        /// Create a Path from a List<Point>
        /// </summary>
        /// <param name="listPoint"></param>
        public Path CreatePathByPointList(List<Point> listPoint)
        {
            Path path = new Path()
            {
                Stroke = Brushes.DeepSkyBlue,   //画笔，设置为深天蓝色   
                StrokeThickness = 2,            //画笔厚度   
                StrokeDashArray = new DoubleCollection(new double[] { 1, 1, 1 }),   //画出三个1px一组的虚线
                Data = new PathGeometry()
            };

            for (int i = 0; i < listPoint.Count; i++)
            {
                PathGeometry pathGeometry = path.Data as PathGeometry;   //得到从路径Data属性中得到路径几何对象(PathGeometry)   
                var figureCollection = pathGeometry.Figures; //从路径几何对象中得到 PathFigure 集合   

                PathFigure figure = null;
                if (figureCollection.Count == 0)
                {
                    figure = new PathFigure();
                    figure.StartPoint = listPoint[0];    //设置起点   
                    figureCollection.Add(figure);
                }
                else
                {
                    figure = figureCollection.Last();    //得到最后一个 PathFigure 对象   
                }

                var segments = figure.Segments; //从得到的 PathFigure 对象中得到路径段(PathSegment)集合   
                LineSegment segment = null;     //此范例使用线段 LineSegment 对象   
                Point lastLocation = new Point(0, 0);   //最后一次点坐标   
                if (segments.Count > 0)
                {
                    segment = segments.Last() as LineSegment;   //得到路径集合中最后一条线段   
                }
                lastLocation = segment != null ? segment.Point : lastLocation;  //得到最后那条线段的终点坐标   
                var newLocation = listPoint[i];

                LineSegment newsegment = new LineSegment(newLocation, true);    //新线段。LineSegment构造函数的第二个布尔值代表是否被画出来   
                segments.Add(newsegment);   //将新线段添加到路径集合中   

                if (!CanvasMap.Children.Contains(path))  //lineArea 是此类(Window1.xaml)中的画布(Canvas)对象   
                {
                    CanvasMap.Children.Add(path);        //如果没有这个路径就将其添加到画布上，否则画布会自动根据Path.Data数据调整路径的显示   
                    Canvas.SetZIndex(path, 0);
                }
            }

            return path;
        }

        /// <summary>
        /// Create a Path from List<Point> which appoint from point and to point
        /// </summary>
        /// <param name="listPoint"></param>
        /// <param name="formPoint"></param>
        /// <param name="toPoint"></param>
        /// <returns></returns>
        public Path CreateFormToPath(List<Point> listPoint, int formPoint, int toPoint)
        {
            List<Point> _ListPoint = new List<Point>();
            Path newPath;
            if (formPoint < toPoint)//sequence
            {
                for (int i = formPoint; i <= toPoint; i++)
                {
                    _ListPoint.Add(listPoint[i]);
                }

                newPath = CreatePathByPointList(_ListPoint);
            }
            else//antitone
            {
                for (int i = formPoint; i >= toPoint; i--)
                {
                    _ListPoint.Add(listPoint[i]);
                }

                newPath = CreatePathByPointList(_ListPoint);
            }

            return newPath;
        }

        /// <summary>
        /// Get Animation spend time form List<Point>
        /// </summary>
        /// <param name="listPoint"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public int GetTimeSpendByPointList(List<Point> listPoint, double speed)
        {
            double totalDistance = 0;
            for (int i = 0; i < listPoint.Count - 1; i++)
            {
                Point p1 = listPoint[i];
                Point p2 = listPoint[i + 1];
                double value = Math.Sqrt(Math.Abs(p1.X - p2.X) * Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) * Math.Abs(p1.Y - p2.Y));
                totalDistance += value;
            }

            return Convert.ToInt32(totalDistance / speed);
        }

        /// <summary>
        /// Get Animation spend time form List<Point> which appoint from point and to point
        /// </summary>
        /// <param name="listPoint"></param>
        /// <param name="speed"></param>
        /// <param name="formPoint"></param>
        /// <param name="toPoint"></param>
        /// <returns></returns>
        public double GetTimeSpendByPointListFromTo(List<Point> listPoint, double speed, int formPoint, int toPoint)
        {
            double totalDistance = 0;
            List<Point> _ListPoint = new List<Point>();

            if (formPoint < toPoint)//正序
            {
                for (int i = formPoint; i <= toPoint; i++)
                {
                    _ListPoint.Add(listPoint[i]);
                }
            }
            else//倒序
            {
                for (int i = formPoint; i >= toPoint; i--)
                {
                    _ListPoint.Add(listPoint[i]);
                }
            }

            for (int i = 0; i < _ListPoint.Count - 1; i++)
            {
                Point p1 = listPoint[i];
                Point p2 = listPoint[i + 1];
                double value = Math.Sqrt(Math.Abs(p1.X - p2.X) * Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) * Math.Abs(p1.Y - p2.Y));
                totalDistance += value;
            }

            return totalDistance / speed;
        }

        /// <summary>
        /// Remove Last Segment from target path
        /// </summary>
        /// <param name="path"></param>
        private void RemoveLastSegment(Path path)
        {
            if (path != null)
            {
                if (path.Data != null)
                {
                    var pathGeometry = path.Data as PathGeometry;
                    if (pathGeometry.Figures.Count > 0)
                    {
                        var figure = pathGeometry.Figures.Last();
                        if (figure.Segments.Count > 0)
                        {
                            var segment = figure.Segments.Last();
                            figure.Segments.Remove(segment);
                        }
                    }
                }
            }
        }
        #endregion

        #region 播放动画
        /// <summary>
        /// Play Animation dependence a Button
        /// </summary>
        /// <param name="path"></param>
        /// <param name="empButton"></param>
        /// <param name="timeSp"></param>
        public void StartAnimation(Path path, Button empButton, int timeSp)
        {
            Storyboard stMain = new Storyboard();

            AnimationInfo aniInfo = new AnimationInfo();
            aniInfo.EmpName = (string)empButton.ToolTip;
            aniInfo.SBMain = stMain;
            aniInfo.UserControlBase = userControlMap;
            aniInfo.btnEmp = empButton;
            aniInfo.animaPath = path;
            aniInfo.CanvasBase = CanvasMap;
            aniInfo.ViewBoxBase = ViewBoxMap;
            aniInfo.CanvasRootBase = CanvasRootMap;
            aniInfo.FollowingEvent += new AnimationInfo.FollowingHandle(aniInfo_FollowingEvent);
            aniInfo.ReplayEvent += new AnimationInfo.ReplayHandle(aniInfo_ReplayEvent);

            stMain.Completed += (o, s) =>
            {
                CanvasMap.Children.Remove(empButton);//移除动画button
                CanvasMap.Children.Remove(path);//移除path
                OnFinishedAnimationEvent(string.Format("{0}的轨迹播放完毕！", empButton.ToolTip));//通知播放完毕，没有订阅这个事件则可以不处理
                aniInfo.IsFollowing = false;//取消跟踪
                aniInfo.IsPaused = true;//暂停，为了重播时直接转成播放风格，而不需要转成暂停再转成播放
                aniInfo.Stop();
                CheckFinishedAllAnimation();
            };

            Canvas.SetTop(empButton, -empButton.Height / 2);
            Canvas.SetLeft(empButton, -empButton.Width / 2);

            TranslateTransform translate = new TranslateTransform();
            ((TransformGroup)empButton.RenderTransform).Children.Add(translate);

            string RenderName = string.Format("translate{0}", RenderFlag);
            userControlMap.RegisterName(RenderName, translate);
            RenderFlag++;

            DoubleAnimationUsingPath animationX = new DoubleAnimationUsingPath();
            animationX.PathGeometry = path.Data.GetFlattenedPathGeometry();
            animationX.Source = PathAnimationSource.X;
            animationX.Duration = new Duration(TimeSpan.FromSeconds(timeSp));

            DoubleAnimationUsingPath animationY = new DoubleAnimationUsingPath();
            animationY.PathGeometry = path.Data.GetFlattenedPathGeometry();
            animationY.Source = PathAnimationSource.Y;
            animationY.Duration = animationX.Duration;

            Storyboard.SetTargetProperty(animationX, new PropertyPath(TranslateTransform.XProperty));
            Storyboard.SetTargetProperty(animationY, new PropertyPath(TranslateTransform.YProperty));

            Storyboard.SetTargetName(animationX, RenderName);
            Storyboard.SetTargetName(animationY, RenderName);

            stMain.Children.Add(animationX);
            stMain.Children.Add(animationY);

            stMain.Begin(userControlMap, true);

            aniInfo.IsPaused = false;
            ListStoryboard.Add(stMain);
            ListButton.Add(empButton);
            ListPath.Add(path);
            CollectionAnmInfo.Add(aniInfo);
        }

        #region 跟踪、重播、全结束检查
        /// <summary>
        /// 控制同一时刻只有一个轨迹自动跟踪
        /// </summary>
        /// <param name="ai"></param>
        void aniInfo_FollowingEvent(AnimationInfo ai)
        {
            foreach (AnimationInfo item in CollectionAnmInfo)
            {
                if (item != ai)
                {
                    item.IsFollowing = false;
                }
            }
        }

        /// <summary>
        /// 重播轨迹
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="story"></param>
        /// <param name="path"></param>
        /// <param name="empButton"></param>
        void aniInfo_ReplayEvent(AnimationInfo ai, Storyboard story, Path path, Button empButton)
        {
            CanvasMap.Children.Add(path);
            CanvasMap.Children.Add(empButton);

            Canvas.SetZIndex(path, 0);
            Canvas.SetTop(empButton, -empButton.Height / 2);
            Canvas.SetLeft(empButton, -empButton.Width / 2);

            story.Begin(userControlMap, true);

            ListButton.Add(empButton);
            ListPath.Add(path);
        }

        /// <summary>
        /// 每个轨迹播放完后检查是不是所有动画都已经结束
        /// </summary>
        private void CheckFinishedAllAnimation()
        {
            foreach (Storyboard item in ListStoryboard)
            {
                if (item.GetCurrentState(userControlMap) != ClockState.Filling)
                {
                    IsFinishedAll = false;
                    return;
                }
            }
            IsFinishedAll = true;
        }
        #endregion

        #region 加载ListBox源
        public void LoadAnimationInfoCollection()
        {
            //实现ListBox动画加载效果
            loadAnInfoCount = 0;
            timerLoadAB.Enabled = true;
            timerLoadAB.Start();
        }

        private void timerLoadAB_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (loadAnInfoCount >= CollectionAnmInfo.Count)
            {
                timerLoadAB.Stop();
                timerLoadAB.Enabled = false;
                return;
            }
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                CollectionAnmInfoSource.Add(CollectionAnmInfo[loadAnInfoCount++]);
            }));
        }
        #endregion

        #region 卸载ListBox源
        /// <summary>
        /// 卸载ListBox源，在CollectionAnmInfoSource执行clear之前要执行此方法展现向左飞出的效果
        /// </summary>
        public void UnLoadAnimationInfoCollection()
        {
            tsUnload = TimeSpan.FromSeconds(0);
            for (int i = 0; i < ListBoxMap.Items.Count; i++)
            {
                UnLoadAnimationSource(i);
            }
        }

        private void UnLoadAnimationSource(int index)
        {
            Storyboard sbTemp = new Storyboard();
            sbTemp.AccelerationRatio = 0.7;
            sbTemp.DecelerationRatio = 0.3;
            sbTemp.Completed += (o, s) =>
            {
                if (index == ListBoxMap.Items.Count - 1)
                {
                    CollectionAnmInfoSource.Clear();
                }
            };
            var g = ListBoxMap.ItemContainerGenerator;
            ListBoxItem item = g.ContainerFromIndex(index) as ListBoxItem;
            if (item == null)
            {
                return;
            }
            TranslateTransform translate = new TranslateTransform();
            item.RenderTransform = translate;

            string RenderName = string.Format("translate{0}", RenderFlag);
            RenderFlag++;
            userControlMap.RegisterName(RenderName, translate);
            Duration duration = new Duration(TimeSpan.FromSeconds(0.3));
            DoubleAnimation daMargin = new DoubleAnimation(-200, duration);
            daMargin.BeginTime = tsUnload;
            tsUnload += TimeSpan.FromSeconds(0.1);
            Storyboard.SetTargetProperty(daMargin, new PropertyPath(TranslateTransform.XProperty));
            Storyboard.SetTargetName(daMargin, RenderName);
            sbTemp.Children.Add(daMargin);

            sbTemp.Begin(userControlMap);
        }
        #endregion
        #endregion

        #region 停止动画
        /// <summary>
        /// Stop all animation
        /// </summary>
        public void StopAllAnimation()
        {
            //Stop animation
            foreach (Storyboard item in ListStoryboard)
            {
                item.Pause(userControlMap);
            }
            ListStoryboard.Clear();

            //Remove animation button
            foreach (Button item in ListButton)
            {
                CanvasMap.Children.Remove(item);
            }
            ListButton.Clear();

            //Remove animation path
            foreach (Path item in ListPath)
            {
                CanvasMap.Children.Remove(item);
            }
            ListPath.Clear();
            CollectionAnmInfo.Clear();

            UnLoadAnimationInfoCollection();
        }

        /// <summary>
        /// Pause all animation
        /// </summary>
        public void PauseAllAnimation()
        {
            foreach (Storyboard item in ListStoryboard)
            {
                item.Pause(userControlMap);
            }

            foreach (AnimationInfo item in CollectionAnmInfo)
            {
                item.IsPaused = true;
            }
        }

        /// <summary>
        /// Resume all animation
        /// </summary>
        public void ResumeAllAnimation()
        {
            foreach (Storyboard item in ListStoryboard)
            {
                item.Resume(userControlMap);
            }

            foreach (AnimationInfo item in CollectionAnmInfo)
            {
                item.IsPaused = false;
            }
        }
        #endregion
        #endregion
    }
}
