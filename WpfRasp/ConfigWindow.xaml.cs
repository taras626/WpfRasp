using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfRasp
{
    /// <summary>
    /// Логика взаимодействия для ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private Dictionary<string, string> bufferFaculties; 
        
        public ConfigWindow()
        {
            bufferFaculties = ConfigManager.config.FacultiesNames;
            List<string> fullNameFaculties = ConfigManager.config.FacultiesNames.Keys.ToList();
            
            InitializeComponent();

            cb_FullNameFaculty.ItemsSource = fullNameFaculties;
            cb_FullNameFaculty.SelectedIndex = 0;

            tb_ShortFaculty.Text = GetShortNameByFaculty(cb_FullNameFaculty.SelectedItem.ToString());

            for (int i = 0; i < ConfigManager.config.TimeOfLessons.Count; i++) 
            {
                tb_Time.AppendText($"{ConfigManager.config.TimeOfLessons[i]}");
                if(i != ConfigManager.config.TimeOfLessons.Count-1)
                    tb_Time.AppendText($"\n");
            }

        }

        private void ConfigWindow_Closed(object sender, EventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("Вы хотите сохранить изменения?", "Сохренение", MessageBoxButton.YesNoCancel);
            if (dr == MessageBoxResult.Yes) 
            {
                ConfigManager.config.FacultiesNames = bufferFaculties;
                ConfigManager.config.TimeOfLessons = tb_Time.Text.Split("\n").ToList();

                DataManager.SaveResultJson(ConfigManager.CONFIGPATH , ConfigManager.config);
            }
        }

        private string GetShortNameByFaculty(string facName) 
        {
            return ConfigManager.config.FacultiesNames.FirstOrDefault(x => x.Key.Equals(facName)).Value;
        }

        private void cb_FullNameFaculty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tb_ShortFaculty.Text = bufferFaculties.FirstOrDefault(x => x.Key.Equals(cb_FullNameFaculty.SelectedItem.ToString())).Value;
        }

        private void tb_ShortFaculty_TextChanged(object sender, TextChangedEventArgs e)
        {
            bufferFaculties[cb_FullNameFaculty.SelectedItem.ToString()] = tb_ShortFaculty.Text;
        }
    }
}
