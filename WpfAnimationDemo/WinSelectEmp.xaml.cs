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
using System.Data;
using System.Collections.ObjectModel;
using DataAccess;
using System.Text.RegularExpressions;

namespace WpfAnimationDemo
{
    /// <summary>
    /// WinSelectEmp.xaml 的交互逻辑
    /// </summary>
    public partial class WinSelectEmp : Window
    {
        #region 变量
        BLL bll = new BLL();
        List<Deptment> m_deptList = new List<Deptment>();
        List<Duty> m_dytuList = new List<Duty>();
        List<WorkType> m_workTypeList = new List<WorkType>();
        #endregion

        #region 委托
        public delegate void WinClosingHandle(ObservableCollection<EmpMover> empCollection);
        public event WinClosingHandle WinClosingEvent;
        public void OnWinClosingEvent(ObservableCollection<EmpMover> empCollection)
        {
            if (WinClosingEvent != null)
            {
                WinClosingEvent(empCollection);
            }
        }
        #endregion

        #region 属性
        public DataTable DTEmp { get; set; }                  //人员信息
        public DataTable DTDept { get; set; }                 //部门信息
        public DataTable DTDuty { get; set; }                 //职务信息
        public DataTable DTWorkType { get; set; }             //工种信息
        public ObservableCollection<EmpMover> EmpUnSelectedCollection { get; set; }
        public ObservableCollection<EmpMover> EmpSelectedCollection { get; set; }
        #endregion

        #region 枚举

        #endregion

        #region 构造函数
        public WinSelectEmp()
        {
            this.InitializeComponent();

            // 在此点之下插入创建对象所需的代码。
            EmpUnSelectedCollection = new ObservableCollection<EmpMover>();
            EmpSelectedCollection = new ObservableCollection<EmpMover>();
            dgMain.ItemsSource = EmpUnSelectedCollection;
            dgMainSelected.ItemsSource = EmpSelectedCollection;
        }
        #endregion

        #region 业务
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            ExecuteQuery();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            cmbDept.SelectedIndex = 0;
            cmbDuty.SelectedIndex = 0;
            cmbWorkType.SelectedIndex = 0;
            tbBlock.Text = "";
            tbName.Text = "";
        }

        private void tbBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex r = new Regex(@"^[0-9]*$");

            if (tbBlock.Text.Length != 0)
            {
                if (!r.IsMatch(tbBlock.Text))
                {
                    MessageBox.Show("只能输入数字", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    tbBlock.Text = tbBlock.Text.Remove(tbBlock.SelectionStart - 1, 1);
                    tbBlock.Select(this.tbBlock.Text.Length, 0);
                }
            }
        }

        private void tbBlock_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnQuery_Click(null, null);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chb = (CheckBox)sender;
            if ((bool)chb.IsChecked)
            {
                foreach (EmpMover item in dgMain.Items)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (EmpMover item in dgMain.Items)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void CheckBoxSelected_Click(object sender, RoutedEventArgs e)
        {

            CheckBox chb = (CheckBox)sender;
            if ((bool)chb.IsChecked)
            {
                foreach (EmpMover item in dgMainSelected.Items)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (EmpMover item in dgMainSelected.Items)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void dgMain_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo dgc = dgMain.CurrentCell;
            if (dgc.Column == null)
            {
                return;
            }
            if (dgc.Column.GetType() == typeof(DataGridCheckBoxColumn))
            {
                //多选
                if (dgMain.SelectedItems.Count > 1)
                {
                    foreach (EmpMover item in dgMain.SelectedItems)
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                        else
                        {
                            item.IsSelected = true;
                        }
                    }
                }
                //单选
                else
                {
                    EmpMover emp = (EmpMover)dgMain.SelectedItem;
                    if (emp == null)
                    {
                        return;
                    }
                    if (emp.IsSelected)
                    {
                        emp.IsSelected = false;
                    }
                    else
                    {
                        emp.IsSelected = true;
                    }
                }
            }

            //SetChbALlCheckState();
        }

        private void dgSelected_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo dgc = dgMainSelected.CurrentCell;
            if (dgc.Column == null)
            {
                return;
            }
            if (dgc.Column.GetType() == typeof(DataGridCheckBoxColumn))
            {
                //多选
                if (dgMainSelected.SelectedItems.Count > 1)
                {
                    foreach (EmpMover item in dgMainSelected.SelectedItems)
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                        else
                        {
                            item.IsSelected = true;
                        }
                    }
                }
                //单选
                else
                {
                    EmpMover emp = (EmpMover)dgMainSelected.SelectedItem;
                    if (emp == null)
                    {
                        return;
                    }
                    if (emp.IsSelected)
                    {
                        emp.IsSelected = false;
                    }
                    else
                    {
                        emp.IsSelected = true;
                    }
                }
            }

            //SetChbALlCheckState();
        }

        private void rbtPoint_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbt = (RadioButton)sender;
            if (rbt.Content.Equals("未加入人员"))
            {
                //btnAdd.Visibility = System.Windows.Visibility.Visible;
                //btnRemove.Visibility = System.Windows.Visibility.Collapsed;
                //dgMain.ItemsSource = EmpUnSelectedCollection;
            }
            else
            {
                //btnAdd.Visibility = System.Windows.Visibility.Collapsed;
                //btnRemove.Visibility = System.Windows.Visibility.Visible;
                //dgMain.ItemsSource = EmpSelectedCollection;
            }

            SetChbALlCheckState();
        }

        private void SetChbALlCheckState()
        {
            if (dgMain.Items.Count == 0)
            {
                SetChbALlCheckState(false);
                return;
            }

            foreach (EmpMover item in dgMain.Items)
            {
                if (!item.IsSelected)
                {
                    SetChbALlCheckState(false);
                    return;
                }
            }

            SetChbALlCheckState(true);
        }

        private void SetChbALlCheckState(bool isCheck)
        {
            CheckBox chb = GetVisualControl.GetChild<CheckBox>(dgMain);
            if (chb != null)
            {
                if (chb.Name.Equals("chbSelectAll"))
                {
                    if (isCheck)
                    {
                        chb.IsChecked = true;
                    }
                    else
                    {
                        chb.IsChecked = false;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rbtNopoint.IsChecked = true;
            LoadDatatables();
            cmbDept.SelectedIndex = 0;
            cmbDuty.SelectedIndex = 0;
            cmbWorkType.SelectedIndex = 0;
            ExecuteQuery();
        }

        private void LoadDatatables()
        {
            //DTEmp = bll.GetEmpList("");
            DTDept = bll.GetDept();
            DTDuty = bll.GetDuty();
            DTWorkType = bll.GetWorkType();

            //dept
            m_deptList.Add(new Deptment() { DeptID = "0", DeptName = "所有" });
            foreach (DataRow item in DTDept.Rows)
            {
                Deptment dep = new Deptment()
                {
                    DeptID = item[0].ToString(),
                    DeptName = item[1].ToString()
                };
                m_deptList.Add(dep);
            }
            cmbDept.ItemsSource = m_deptList;
            cmbDept.SelectedIndex = 0;

            //duty
            m_dytuList.Add(new Duty() { DutyID = "0", DutyName = "所有" });
            foreach (DataRow item in DTDuty.Rows)
            {
                Duty duty = new Duty()
                {
                    DutyID = item[0].ToString(),
                    DutyName = item[1].ToString()
                };
                m_dytuList.Add(duty);
            }
            cmbDuty.ItemsSource = m_dytuList;
            cmbDuty.SelectedIndex = 0;

            //WorkType
            m_workTypeList.Add(new WorkType() { WorkTypeID = "0", WorkTypeName = "所有" });
            foreach (DataRow item in DTWorkType.Rows)
            {
                WorkType workType = new WorkType()
                {
                    WorkTypeID = item[0].ToString(),
                    WorkTypeName = item[1].ToString()
                };
                m_workTypeList.Add(workType);
            }
            cmbWorkType.ItemsSource = m_workTypeList;
            cmbWorkType.SelectedIndex = 0;
        }

        private void ExecuteQuery()
        {
            string deptName = ((Deptment)cmbDept.SelectedItem).DeptName;
            string dutyName = ((Duty)cmbDuty.SelectedItem).DutyName;
            string workTypeName = ((WorkType)cmbWorkType.SelectedItem).WorkTypeName;
            string blockID = tbBlock.Text.Trim();
            string empName = tbName.Text.Trim();

            DTEmp = bll.GetEmpDataTable(deptName, dutyName, workTypeName, blockID, empName);
            if ((bool)rbtNopoint.IsChecked)
            {
                EmpUnSelectedCollection.Clear();

                if (EmpSelectedCollection.Count == 0)
                {
                    foreach (DataRow item in DTEmp.Rows)
                    {
                        EmpUnSelectedCollection.Add(new EmpMover()
                        {
                            Name = item["员工姓名"].ToString(),
                            EmpID = item["ID"].ToString(),
                            BlockID = item["标识卡"].ToString(),
                            DeptName = item["员工部门"].ToString(),
                            DutyName = item["职务名"].ToString(),
                            WorkTypeName = item["工种"].ToString(),
                        });
                    }
                }
                else
                {
                    foreach (DataRow item in DTEmp.Rows)
                    {
                        if (CheckExistsEmpByEmpName(item["员工姓名"].ToString()))
                        {
                            continue;
                        }

                        EmpUnSelectedCollection.Add(new EmpMover()
                        {
                            Name = item["员工姓名"].ToString(),
                            EmpID = item["ID"].ToString(),
                            BlockID = item["标识卡"].ToString(),
                            DeptName = item["员工部门"].ToString(),
                            DutyName = item["职务名"].ToString(),
                            WorkTypeName = item["工种"].ToString(),
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Check exists Empmover in EmpSelectedCollection for unload Empmover to EmpUnSelectedCollection
        /// </summary>
        /// <param name="empName"></param>
        /// <returns></returns>
        private bool CheckExistsEmpByEmpName(string empName)
        {
            bool isExists = false;
            foreach (EmpMover item in EmpSelectedCollection)
            {
                if (item.Name.Equals(empName))
                {
                    isExists = true;
                    break;
                }
                else
                {
                    isExists = false;
                }
            }
            return isExists;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (EmpMover item in EmpUnSelectedCollection)
            {
                if (item.IsSelected)
                {
                    EmpSelectedCollection.Add(item);
                }
            }

            ExecuteQuery();

            SetChbALlCheckState();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            List<EmpMover> listSelectedMover = new List<EmpMover>();

            foreach (EmpMover item in EmpSelectedCollection)
            {
                if (item.IsSelected)
                {
                    listSelectedMover.Add(item);
                }
            }

            if (listSelectedMover.Count != 0)
            {
                foreach (EmpMover item in listSelectedMover)
                {
                    EmpSelectedCollection.Remove(item);
                }
            }

            SetChbALlCheckState();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnWinClosingEvent(EmpSelectedCollection);
        }
        #endregion     
    }
}