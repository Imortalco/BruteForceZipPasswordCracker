using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BruteForceZipPasswordCracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void CrackPassword()
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                
                var passwordQueue = new  BlockingCollection<string>();

                var generator = new PasswordGenerator(passwordQueue, tokenSource.Token, "0");
   
                generator.Run();

                TaskCompletionSource<string> passwordResponse = new TaskCompletionSource<string>();
               
                List<PasswordFinder> passwordFinders = new List<PasswordFinder>();
                for (int i = 1; i <= int.Parse(threadBox.Text); i++)
                {
                    var passwordFinder = new PasswordFinder(Input.Text, passwordResponse, passwordQueue, tokenSource.Token,i);
                    passwordFinders.Add(passwordFinder);
                    passwordFinder.Run();
                }

                bool Handled = true;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                try
                {
                    Task<string> passwordTasks = passwordResponse.Task;
                    passwordTasks.Wait();
                    
                    Output.Content = "Password is " + passwordTasks.Result;
                    ShowLogs(passwordFinders);
                    
                }
                catch (Exception ex)
                {
                    Handled = HandleError(ex);
                }
                finally
                {
                    tokenSource.Cancel();
                    stopWatch.Stop();
                    if (Handled)
                        MessageBox.Show("Time: " + stopWatch.Elapsed.ToString("mm\\:ss\\:fffffff"), "Finding Time", MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowLastLog(passwordFinders);
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
            listBox.Items.Clear();
            Output.Content = "";
            CrackPassword();
        }

        private void Input_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Input.Text = openFileDialog.FileName;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ShowLogs(List<PasswordFinder> passwordFinders)
        {
            int n = passwordFinders.Min(s => s.log.Count);
            for (int i = n - n*19/20; i < n; i++)
            {
                for (int j = 0; j < passwordFinders.Count; j++)
                {
                    listBox.Items.Add(passwordFinders[j].log[i]);
                }
            }
        }

        private void ShowLastLog(List<PasswordFinder> passwordFinders)
        {
            string lastLog = "";
            foreach (PasswordFinder passwordFinder in passwordFinders)
            {
                lastLog = passwordFinder.log.FirstOrDefault(s => s.Contains("found"));
                if (lastLog != null)
                {
                    break;
                }
            }
            listBox.Items.Add(lastLog);
            listBox.SelectedItem = lastLog;
        }
    }
}
