using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfRasp.Models;

namespace WpfRasp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int indexLastSelectedFaculty = 0;

        public MainWindow()
        {
            InitializeComponent();

            List<string> months = new List<string>();

            for (int i = 1; i <= 12; i++)
            {
                string month = new DateTime(DateTime.Now.Year, i, 1).ToString("MMMM");
                month = month.Replace(month[0], Char.ToUpper(month[0]));
                months.Add(month);
            }

            cb_Month.ItemsSource = months;

            cb_Month.SelectedIndex = 0;

            tb_Year.Text = DateTime.Now.Year.ToString();

            rb_OneGroup.IsChecked = true;

            InitializeData();
        }

        private void cb_Faculties_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            indexLastSelectedFaculty = cb_Faculties.SelectedIndex == -1 ? indexLastSelectedFaculty : cb_Faculties.SelectedIndex;
            GetGroups();
        }

        private async void btn_GetTimeTable_Click(object sender, RoutedEventArgs e)
        {
            btn_ConfigEdit.Visibility = Visibility.Hidden;
            pb_WaitingExcel.Visibility = Visibility.Visible;
            int year = -1;
            DateTime date = DateTime.Now;
            if (int.TryParse(tb_Year.Text, out year)) 
            {
                date = new DateTime(year, cb_Month.SelectedIndex + 1, 1);
                ExcelWriter.CreateResult(date, await DataManager.GetAllDataFaculty(new InputRequest(DataManager.GetFacultyByShortName(cb_Faculties.SelectedItem.ToString()).Name, date), ConfigManager.config.PathFaculty));    
            }
            else
                MessageBox.Show("Введите корректно год!!!");
            pb_WaitingExcel.Visibility = Visibility.Hidden;
            btn_ConfigEdit.Visibility = Visibility.Visible;

            MessageBox.Show("Таблица создана на Рабочем столе!");
        }

        private void btn_AddNewGroup_Click(object sender, RoutedEventArgs e)
        {
            List<string> groupsName = new List<string>();
            if ((bool)rb_OneGroup.IsChecked)
            {
                groupsName.Add(tb_NewGroup.Text);
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();                
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Любые файлы (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                    {
                        while (!reader.EndOfStream) 
                        {
                            groupsName.Add(reader.ReadLine());
                        }
                    }
                }
            }

            GetResultAsync(groupsName);

            tb_NewGroup.Text = string.Empty;
        }

        private async Task GetResultAsync(List<string> groupsName) 
        {
            foreach (string groupName in groupsName)
            {
                await DataManager.AddNewGroupAsync(groupName);
            }
            InitializeData();
        }

        private void InitializeData() 
        {
            if (ConfigManager.config == null)
            {
                int conf = ConfigManager.ReadConf();
                if (conf == -1)
                {
                    MessageBox.Show("У вас пропал конфиг, обратитесь в ОЦБТ!");
                    Environment.Exit(1);
                }
            }

            try
            {
                bool isEmpty = true;

                ConfigManager.ReadConf();

                using (StreamReader sr = new StreamReader(ConfigManager.config.PathFaculty))
                {
                    string allData = sr.ReadToEnd();
                    if (!allData.Equals(""))
                    {
                        DataManager.Faculties = JsonSerializer.Deserialize<List<Faculty>>(allData);
                        isEmpty = false;
                    }
                }

                if (!isEmpty)
                {
                    List<string> facultyNames = new List<string>();

                    foreach (var fac in DataManager.Faculties)
                        facultyNames.Add(ConfigManager.config.FacultiesNames.FirstOrDefault(x => x.Key.Equals(fac.Name)).Value);

                    cb_Faculties.ItemsSource = facultyNames;
                    cb_Faculties.SelectedItem = cb_Faculties.Items[indexLastSelectedFaculty];

                    GetGroups();
                }
            }
            catch (IOException e)
            {
                using (StreamWriter wr = new StreamWriter(ConfigManager.config.PathFaculty))
                {
                    wr.Write("");
                }
                using (StreamWriter wr = new StreamWriter(ConfigManager.CONFIGPATH))
                {
                    wr.Write("");
                }
            }
        }

        private void GetGroups() 
        {
            List<string> groupsNames = new List<string>();

            if(cb_Faculties.Items.Count > 0)
                foreach (var group in DataManager.Faculties.FirstOrDefault(x => x.Name.Equals(ConfigManager.config.FacultiesNames.FirstOrDefault(y => y.Value.Equals(cb_Faculties.Items[indexLastSelectedFaculty].ToString())).Key)).Groups)
                    groupsNames.Add(group.label);

            cb_Groups.ItemsSource = groupsNames;
        }

        private void rb_OneGroup_Checked(object sender, RoutedEventArgs e)
        {
            btn_DeleteAllGroups.Content = "Удалить группу";
            btn_AddNewGroup.Content = "Добавить новую группу";
            btn_AddNewGroup.IsEnabled = true;
            tb_NewGroup.IsEnabled = true;
        }

        private void rb_ListGroups_Checked(object sender, RoutedEventArgs e)
        {
            btn_DeleteAllGroups.Content = "Удалить все группы";
            btn_AddNewGroup.Content = "Добавить новые группы";
            btn_AddNewGroup.IsEnabled = true;
            tb_NewGroup.IsEnabled = false;
        }

        private void rb_Faculty_Checked(object sender, RoutedEventArgs e)
        {
            btn_DeleteAllGroups.Content = "Удалить Факультет";
            btn_AddNewGroup.IsEnabled = false;
            tb_NewGroup.IsEnabled = false;
        }

        private void btn_DeleteAllGroups_Click(object sender, RoutedEventArgs e)
        {
            if (rb_ListGroups.IsChecked == true)
            {
                MessageBoxResult dr = MessageBox.Show("Вы уверены, что хотите удалить ВСЕ группы?", "Проверка", MessageBoxButton.YesNo);
                if (dr == MessageBoxResult.Yes) 
                    DataManager.DeleteGroups();                
            }
            else 
            {
                if (rb_OneGroup.IsChecked == true)
                {
                    MessageBoxResult dr = MessageBox.Show($"Вы уверены, что хотите удалить группу {tb_NewGroup.Text}?", "Проверка", MessageBoxButton.YesNo);
                    if (dr == MessageBoxResult.Yes)
                    {
                        DataManager.DeleteGroup(tb_NewGroup.Text);
                        tb_NewGroup.Text = "";
                    }
                }
                else 
                {
                    MessageBoxResult dr = MessageBox.Show($"Вы уверены, что хотите удалить группы всего факультета {cb_Faculties.SelectedItem}?", "Проверка", MessageBoxButton.YesNo);
                    if (dr == MessageBoxResult.Yes)
                    {
                        DataManager.DeleteFaculty(DataManager.GetFacultyByShortName(cb_Faculties.SelectedItem.ToString()).Name);
                        tb_NewGroup.Text = "";
                    }
                }
            }
            cb_Faculties.ItemsSource = new List<string>();
            InitializeData();
        }

        private void btn_ConfigEdit_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.ShowDialog();
            InitializeData();
        }
    }
}
//await DataManager.GetAllDataFaculty(new InputRequest(inputStr, dateTime), "faculties.json")