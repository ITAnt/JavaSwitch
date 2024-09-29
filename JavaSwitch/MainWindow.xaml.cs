using JavaSwitch.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace JavaSwitch
{
    public class ListItem
    {
        public string Text { get; set; }
        public bool IsSelected { get; set; }

        public ListItem(string text)
        {
            Text = text;
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ListItem> Items { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            // 从本地加载已有的数据
            LoadDataFromJson();
            // 数据绑定上下文
            DataContext = this;
        }

        private void Button_Add(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                // 设置初始目录
                RootFolder = Environment.SpecialFolder.Desktop
            };
            // 这个方法可以显示文件夹选择对话框
            folderBrowserDialog.ShowDialog();
            // 获取选择的文件夹的全路径名
            string directoryPath = folderBrowserDialog.SelectedPath;

            // 打印出选择的路径
            Console.WriteLine(directoryPath);

            if (string.IsNullOrEmpty(directoryPath))
            {
                return;
            }

            // 持久化到本地
            var newItem = new ListItem(directoryPath);
            Items.Add(newItem);
            SaveDataToJson();
        }

        /// <summary>
        /// 删除一项
        /// </summary>
        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            var deleteButton = sender as System.Windows.Controls.Button;
            if (deleteButton != null)
            {
                var listItem = deleteButton.DataContext as ListItem;
                if (listItem != null)
                {
                    Items.Remove(listItem);
                    SaveDataToJson();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // 打开环境变量设置页面 rundll32 sysdm.cpl,EditEnvironmentVariables
            Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");
            //string originalPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            //System.Windows.MessageBox.Show("Original Path: " + originalPath);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as System.Windows.Controls.RadioButton;
            if (radioButton != null)
            {
                var listItem = radioButton.DataContext as ListItem;
                if (listItem != null)
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = item == listItem;
                        if (item.IsSelected)
                        {
                            // 设置环境变量
                            // 执行bash命令：setx JAVA_PATH "%JAVA_HOME%\bin;%JAVA_HOME%\lib\dt.jar;%JAVA_HOME%\lib\tools.jar;" /M
                            Process.Start("cmd", "/c setx JAVA_PATH \"" + item.Text + "\\bin;" + item.Text + "\\lib\\dt.jar;" + item.Text + "\\lib\\tools.jar;\" /M");
                            // 执行bash命令：refreshenv.cmd
                            Process.Start("cmd", "/c refreshenv.cmd");
                        }
                    }
                }
            }
            SaveDataToJson();
        }

        private string ENV_DATA = "JavaEnvironment.json";
        /// <summary>
        /// 保存数据到本地
        /// </summary>
        private void SaveDataToJson()
        {
            var json = JsonConvert.SerializeObject(Items, Formatting.Indented);
            File.WriteAllText(ENV_DATA, json);
        }

        /// <summary>
        /// 从本地加载数据
        /// </summary>
        private void LoadDataFromJson()
        {
            if (File.Exists(ENV_DATA))
            {
                var json = File.ReadAllText(ENV_DATA);
                Items = JsonConvert.DeserializeObject<ObservableCollection<ListItem>>(json);
            }
            else
            {
                Items = new ObservableCollection<ListItem>();
            }
        }
    }
}
