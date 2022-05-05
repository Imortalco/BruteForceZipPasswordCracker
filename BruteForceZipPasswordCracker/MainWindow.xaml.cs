using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BruteForceZipPasswordCracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class ZipFileNotExistentException : Exception
        { }

        class ZipFileNotPasswordProtectedException : Exception
        { }

        public void CrackPassword()
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {

                List<Task> taskList = new List<Task>();

                var passwordQueue = new  BlockingCollection<string>();

                var generator = new PasswordGenerator(passwordQueue, tokenSource.Token, "0");
                taskList.Add(generator.Run());

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                TaskCompletionSource<string> passwordResponse = new TaskCompletionSource<string>();
                for (int i = 1; i <= 10; i++)
                {
                    var pwd_consumer = new PasswordFinder(Input.Text, passwordResponse, passwordQueue);
                    taskList.Add(pwd_consumer.Run());
                }

                bool Handled = true;
                try
                {
                    Task<string> passwordTasks = passwordResponse.Task;
                    passwordTasks.Wait();
                    Pass.Visibility = Visibility.Visible;
                    Output.Content = passwordTasks.Result;
                }
                catch (Exception ex)
                {
                    Handled = HandleError(ex);
                }
                finally
                {
                    stopWatch.Stop();
                    tokenSource.Cancel();
                    if (Handled)
                        MessageBox.Show("Time: " + stopWatch.Elapsed.ToString("mm\\:ss\\:fffffff"), "Finding Time", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private bool HandleError(Exception ex)
        {
            
            if(ex is AggregateException)
            {
                switch (ex.InnerException.Message) 
                {
                    case "Exception of type 'BruteForceZipPasswordCracker.PasswordFinder+ZipFileNotExistentException' was thrown.": 
                        MessageBox.Show("Zip file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); break;
                    case "Exception of type 'BruteForceZipPasswordCracker.PasswordFinder+ZipFileNotPasswordProtectedException' was thrown.":
                        MessageBox.Show("Zip file does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error); break;
                }
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pass.Visibility = Visibility.Hidden;
            Output.Content = "";
            CrackPassword();


        }
    }
    
}
