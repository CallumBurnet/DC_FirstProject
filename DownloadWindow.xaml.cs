using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LobbyClient
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private CancellationTokenSource cts;
        private FileListProxy proxy;
        public DownloadWindow(FileListProxy fileListProxy)
        {
            InitializeComponent();
            cts = new CancellationTokenSource();
            proxy = fileListProxy;
        }
        public async void StartDownload(string fileName)
        {
            var progress = new Progress<int>(value =>
            {
                DownloadProgressBar.Value = value;
            });
            try
            {
                await DownloadFile(fileName, progress, cts.Token);
                downloadText.Text = "Done";
                cancelOrDoneButton.Content = "Close";
            } catch (OperationCanceledException) {
                MessageBox.Show("Download has been cancelled");
            }
        }
        private async Task DownloadFile(string fileName, IProgress<int> progress, CancellationToken token)
        {
            await proxy.DownloadFile(fileName, progress, token);
        }
       
        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            this.Close();
        }
    }
}
