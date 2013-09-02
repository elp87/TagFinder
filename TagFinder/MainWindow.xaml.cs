using elp87.TagReader;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;


namespace WpfApplication4
{

    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);


    //Создание делегата и привязка к изменению свойства

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string reportFile = @"D:\testReport.txt";
        BackgroundWorker worker;
        string path;
        string[] _fileNames;

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(progressBar.SetValue);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(reportFile))
            {
                double value = 0;
                foreach (string fileName in _fileNames)
                {
                    Dispatcher.Invoke(updProgress, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, ++value });
                    try
                    {
                        MP3File mp3 = new MP3File(fileName);
                        if (mp3.id3v2.header.flagField.extendedHeader)
                        {
                            file.WriteLine("SUCCESS - " + fileName);
                        }
                    }
                    catch (elp87.TagReader.id3v2.Exceptions.UnsupportedTagVersionException) { }
                    catch (System.Exception ex)
                    {
                        file.WriteLine(fileName + " - " + ex.Message);
                    }
                }
            }
            System.Windows.MessageBox.Show("Закончено");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                path = fbd.SelectedPath;
                _fileNames = Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories);
                progressBar.Maximum = _fileNames.Length;
                progressBar.Value = 0;
                worker.RunWorkerAsync();
            }
        }
    }
}
