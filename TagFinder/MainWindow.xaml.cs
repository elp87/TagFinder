using elp87.TagReader;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace WpfApplication4
{


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

        string reportFile;
        BackgroundWorker worker;
        Image catImage;
        string path;
        string[] _fileNames;
        string _maskString;
        byte[] _mask;

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(progressBar.SetValue);

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(() => addCatImage() ));

            _mask = Encoding.ASCII.GetBytes(_maskString);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(reportFile))
            {
                double value = 0;
                foreach (string fileName in _fileNames)
                {
                    Dispatcher.Invoke(updProgress, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, ++value });
                    try
                    {
                        //MP3File mp3 = new MP3File(fileName);
                        if (ByteArray.FindSubArray(File.ReadAllBytes(fileName), _mask) != -1)//(mp3.id3v2.header.flagField.extendedHeader)
                        {
                            file.WriteLine("SUCCESS - " + fileName);
                        }
                    }
                    catch (elp87.TagReader.id3v2.Exceptions.UnsupportedTagVersionException) { }
                    catch (elp87.TagReader.id3v2.Exceptions.NoID3V2TagException) { }
                    catch (System.Exception ex)
                    {
                        file.WriteLine(fileName + " - " + ex.Message);
                    }
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, new System.Action(() => grid.Children.Remove(catImage) ));
            System.Windows.MessageBox.Show("Закончено");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _maskString = textBoxMask.Text;
            if (_maskString == "")
            {
                System.Windows.MessageBox.Show("Поле маски пустое");
                return;
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Document";
            sfd.DefaultExt = ".txt";
            sfd.Filter = "файл портфеля (*.txt)|*.txt";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    reportFile = sfd.FileName;
                    path = fbd.SelectedPath;
                    _fileNames = Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories);
                    progressBar.Maximum = _fileNames.Length;
                    progressBar.Value = 0;
                    worker.RunWorkerAsync();
                }
            }
        }

        private void addCatImage()
        {
            catImage = new Image();
            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(catImage, new BitmapImage(new System.Uri("walkingcat_white.gif", System.UriKind.RelativeOrAbsolute)));
            catImage.Width = 300;
            catImage.Height = 300;
            catImage.Margin = new Thickness(0, 170, 0, 0);
            catImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            catImage.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            grid.Children.Add(catImage);
        }
    }
}
