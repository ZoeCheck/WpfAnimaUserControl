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
using System.Windows.Shapes;

namespace WpfAnimationDemo
{
	/// <summary>
	/// WinHisRoute.xaml 的交互逻辑
	/// </summary>
	public partial class WinHisRoute : Window
    {
        #region 变量

        #endregion

        #region 委托

        #endregion

        #region 属性

        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public WinHisRoute()
        {
            this.InitializeComponent();

            // 在此点之下插入创建对象所需的代码。
        }
        #endregion

        #region 业务
        private void radioButtonSepeed_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbtFixStation_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void dpB_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSelectEmp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPlayHisBlock_Click(object sender, RoutedEventArgs e)
        {
            EmpMover emp1 = new EmpMover();
            emp1.Name = "崔国飞";
            emp1.EmpID = "-460349907";

            EmpMover emp2 = new EmpMover();
            emp2.Name = "刘二虎";
            emp2.EmpID = "-1623739144";

            EmpMover emp3 = new EmpMover();
            emp3.Name = "王铁英";
            emp3.EmpID = "-461070800";

            ucAnimation.CollectionEmp.Add(emp1);
            ucAnimation.CollectionEmp.Add(emp2);
            ucAnimation.CollectionEmp.Add(emp3);

            ucAnimation.PlayListHisRoute(DateTime.Parse("2013-08-20 0:00:00"), DateTime.Parse("2013-08-20 23:59:59"));
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ucAnimation.Load();
        }
        #endregion
	}
}