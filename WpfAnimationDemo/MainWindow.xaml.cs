using System;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using System.Collections.ObjectModel;

namespace WpfAnimationDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
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
        public MainWindow()
        {
            InitializeComponent();
            dpB.SelectedDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
        }
        #endregion

        #region 业务
        private void radioButtonSepeed_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbt = (RadioButton)sender;
            switch (rbt.Content.ToString())
            {
                case "慢":
                    ucAnimation.speed = 20;
                    break;
                case "中":
                    ucAnimation.speed = 50;
                    break;
                case "快":
                    ucAnimation.speed = 100;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ucAnimation.Load();
            btnPlayHisBlock.DataContext = ucAnimation.ab;
        }

        private void btnPlayHisBlock_Click(object sender, RoutedEventArgs e)
        {
            btnPlayHisBlock.IsEnabled = false;
            btnPause.IsEnabled = true;
            btnPause.Content = "暂停所有";
            ucAnimation.PlayListHisRoute((DateTime)dpB.SelectedDate, (DateTime)dpE.SelectedDate);
        }

        private void dpB_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            dpE.SelectedDate = ((DateTime)dpB.SelectedDate).AddDays(1).AddSeconds(-1);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ucAnimation.StopAllAnimation();
            btnPause.Content = "暂停所有";
            btnPause.IsEnabled = false;
            btnPlayHisBlock.IsEnabled = true;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Content.Equals("暂停所有"))
            {
                btn.Content = "继续所有";
                ucAnimation.PauseAllAnimation();
            }
            else
            {
                btn.Content = "暂停所有";
                ucAnimation.ResumeAllAnimation();
            }
        }

        private void btnSelectEmp_Click(object sender, RoutedEventArgs e)
        {
            WinSelectEmp ws = new WinSelectEmp();
            ws.WinClosingEvent += new WinSelectEmp.WinClosingHandle(ws_WinClosingEvent);
            ws.ShowDialog();
        }

        void ws_WinClosingEvent(ObservableCollection<EmpMover> empCollection)
        {
            ucAnimation.CollectionEmp = empCollection;
        }

        private void rbtFixStation_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbt = (RadioButton)sender;
            if (rbt.Content.Equals("固定大小"))
            {
                ucAnimation.isStationFixSize = true;
            }
            else
            {
                ucAnimation.isStationFixSize = false;
            }
        }
        #endregion
    }
}
