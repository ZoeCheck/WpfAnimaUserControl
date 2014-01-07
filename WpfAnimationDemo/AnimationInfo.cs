using System;
using Microsoft.Practices.Prism.ViewModel;
using System.Timers;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace WpfAnimationDemo
{
    /// <summary>
    /// Single Animation status infomation include start and stop function as left ListBox ItemsSource
    /// </summary>
    public class AnimationInfo : NotificationObject
    {
        #region 变量
        private string _EmpName;
        private bool _IsPaused;
        public Button btnEmp;
        public Path animaPath;
        private bool isSelected;
        private bool _IsFollowing;
        private CheckBoxStates _CheckBoxState;
        private Storyboard sbMain;
        private double speedRatio = 1;
        private double _AnimationProgress;
        private string _CheckBoxToolTip;

        #endregion

        #region 委托
        public delegate void DeleFunc();
        public delegate void FollowingHandle(AnimationInfo aiUC);
        public event FollowingHandle FollowingEvent;
        public void OnFollowingEvent(AnimationInfo aiUC)
        {
            if (FollowingEvent != null)
            {
                FollowingEvent.Invoke(aiUC);
            }
        }

        public delegate void ReplayHandle(AnimationInfo ai, Storyboard story, Path path, Button empButton);
        public event ReplayHandle ReplayEvent;
        public void OnReplayEvent(AnimationInfo ai, Storyboard story, Path path, Button empButton)
        {
            if (ReplayEvent != null)
            {
                ReplayEvent.Invoke(ai, story, path, empButton);
            }
        }
        #endregion

        #region 属性
        public UserControl UserControlBase { get; set; }
        public Canvas CanvasBase { get; set; }
        public Canvas CanvasRootBase { get; set; }
        public Viewbox ViewBoxBase { get; set; }

        public string EmpName
        {
            get { return _EmpName; }
            set
            {
                _EmpName = value;
                RaisePropertyChanged("EmpName");
            }
        }

        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                _IsPaused = value;
                RaisePropertyChanged("IsPaused");
                if (IsPaused)
                {
                    Pause();
                    btnEmp.Style = UserControlBase.FindResource("ButtonStyleEmpRed") as Style;
                }
                else
                {
                    Resume();
                    btnEmp.Style = UserControlBase.FindResource("ButtonStyleEmp") as Style;
                }
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                if (isSelected)
                {
                    btnEmp.Style = UserControlBase.FindResource("ButtonStyleEmpBlue") as Style;
                }
                else
                {
                    btnEmp.Style = UserControlBase.FindResource("ButtonStyleEmp") as Style;
                }
            }
        }

        public bool IsFollowing
        {
            get { return _IsFollowing; }
            set
            {
                _IsFollowing = value;
                RaisePropertyChanged("IsFollowing");
                if (_IsFollowing)
                {
                    btnEmp.IsEnabled = true;//use for EmpButton shadow animation
                    OnFollowingEvent(this);
                    //timer.Start();
                    SBMain.CurrentTimeInvalidated += SBMain_CurrentTimeInvalidatedFollowing;
                }
                else
                {
                    btnEmp.IsEnabled = false;
                    //timer.Stop();
                    SBMain.CurrentTimeInvalidated -= SBMain_CurrentTimeInvalidatedFollowing;
                }
            }
        }

        public Storyboard SBMain
        {
            get
            {
                return sbMain;
            }
            set
            {
                sbMain = value;
                sbMain.CurrentTimeInvalidated += SBMain_CurrentTimeInvalidated;
            }
        }

        public Point ViewBoxToCanvasRoot
        {
            get
            {
                return new Point(Canvas.GetLeft(ViewBoxBase), Canvas.GetTop(ViewBoxBase));
            }
        }

        public Point CenterPoint
        {
            get
            {
                return new Point(CanvasRootBase.ActualWidth / 2, CanvasRootBase.ActualHeight / 2);
            }
        }

        public CheckBoxStates CheckBoxState
        {
            get
            {
                return _CheckBoxState;
            }
            set
            {
                _CheckBoxState = value;
                RaisePropertyChanged("CheckBoxState");
            }
        }

        public double AnimationProgress
        {
            get { return _AnimationProgress; }
            set
            {
                _AnimationProgress = value;
                RaisePropertyChanged("AnimationProgress");
            }
        }

        public string CheckBoxToolTip
        {
            get { return _CheckBoxToolTip; }
            set
            {
                _CheckBoxToolTip = value;
                RaisePropertyChanged("CheckBoxToolTip");
            }
        }

        public ICommand CommandAddSpeed
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    AddSpeed();
                });
            }
        }

        public ICommand CommandMinusSpeed
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MinusSpeed();
                });
            }
        }
        #endregion

        #region 枚举
        public enum CheckBoxStates
        {
            Pause,
            Resume,
            Stop
        }
        #endregion

        #region 构造函数
        public AnimationInfo()
        {
        }
        #endregion

        #region 业务
        /// <summary>
        /// 实时更新进度条值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SBMain_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            Clock storyboardClock = (Clock)sender;
            AnimationProgress = (double)storyboardClock.CurrentProgress;
        }

        /// <summary>
        /// 自动跟踪，让动画对象居中屏幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SBMain_CurrentTimeInvalidatedFollowing(object sender, EventArgs e)
        {
            FollowRoute();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DeleFunc(FollowRoute));
        }

        /// <summary>
        /// 自动跟踪轨迹方法
        /// </summary>
        private void FollowRoute()
        {
            Point pointInCanvasMain = btnEmp.TransformToAncestor(UserControlBase).Transform(new Point(0, 0));//获取动画控件在MainWindow中的Point
            double x = pointInCanvasMain.X - CenterPoint.X;
            double y = pointInCanvasMain.Y - CenterPoint.Y;
            double xNew = ViewBoxToCanvasRoot.X - x;
            double yNew = ViewBoxToCanvasRoot.Y - y;
            Canvas.SetLeft(ViewBoxBase, xNew);
            Canvas.SetTop(ViewBoxBase, yNew);
        }
       
        /// <summary>
        /// 暂停动画
        /// </summary>
        public void Pause()
        {
            SBMain.Pause(UserControlBase);
            CheckBoxState = CheckBoxStates.Pause;
            CheckBoxToolTip = "继续";
        }

        /// <summary>
        /// 继续动画
        /// </summary>
        public void Resume()
        {
            //重播
            if (SBMain.GetCurrentState(UserControlBase) == ClockState.Filling)
            {
                OnReplayEvent(this, SBMain, animaPath, btnEmp);
            }
            //继续播放
            else
            {
                SBMain.Resume(UserControlBase);
            }
            CheckBoxState = CheckBoxStates.Resume;
            CheckBoxToolTip = "暂停";
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        public void Stop()
        {
            CheckBoxState = CheckBoxStates.Stop;
            CheckBoxToolTip = "重播";
        }

        /// <summary>
        /// 加速
        /// </summary>
        public void AddSpeed()
        {
            SBMain.SetSpeedRatio(UserControlBase, ++speedRatio);
        }

        /// <summary>
        /// 减速
        /// </summary>
        public void MinusSpeed()
        {
            if (speedRatio > 1)
            {
                SBMain.SetSpeedRatio(UserControlBase, --speedRatio);
            }
            else
            {
                if (speedRatio > 0.2 && speedRatio <= 1)
                {
                    speedRatio = Convert.ToDouble((speedRatio - 0.2).ToString("0.00"));
                    SBMain.SetSpeedRatio(UserControlBase, speedRatio);
                }
            }
        }
        #endregion
    }
}
