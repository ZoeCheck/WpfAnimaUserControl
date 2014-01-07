using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.ViewModel;

namespace WpfAnimationDemo
{
    public class EmpMover : NotificationObject
    {
        #region 变量
        public Point nowPoint;
        #endregion

        #region 委托
        //public delegate void PonitChangedHandle(List<Point> listPoint, EmpMover emp);
        //public event PonitChangedHandle PonitChangedEvent;
        //public void OnPonitChangedEvent(List<Point> listPoint, EmpMover emp)
        //{
        //    if (PonitChangedEvent != null)
        //    {
        //        PonitChangedEvent(listPoint, emp);
        //    }
        //}
        #endregion

        #region 属性
        public string Name { get; set; }
        public string EmpID { get; set; }
        public string BlockID { get; set; }
        public string DeptName { get; set; }
        public string DutyName { get; set; }
        public string WorkTypeName { get; set; }

        public Ellipse EllipMover { get; set; }

        /// <summary>
        /// Ellipse宽
        /// </summary>
        public double Width
        {
            get
            {
                return EllipMover.ActualWidth;
            }
        }

        /// <summary>
        /// Ellipse高
        /// </summary>
        public double Height
        {
            get
            {
                return EllipMover.ActualHeight;
            }
        }


        private bool _IsSelected;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public EmpMover()
        {
            //EllipMover = new Ellipse();
            //EllipMover.Width = 15;
            //EllipMover.Height = 15;
            //EllipMover.Fill = new SolidColorBrush(Colors.Lime);
            //EllipMover.ToolTip = Name;
        }
        #endregion

        #region 业务

        #endregion
    }
}
