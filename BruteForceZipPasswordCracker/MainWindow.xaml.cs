using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<string> log { get; set; }
        public MainWindow()
        {
            log = new ObservableCollection<string>();
            InitializeComponent();
            DataContext = this;
        }

        public void CrackPassword()
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                List<Task> taskList = new List<Task>();
                
                var passwordQueue = new  BlockingCollection<string>();

                var generator = new PasswordGenerator(passwordQueue, tokenSource.Token, "0");
   
                taskList.Add(generator.Run());

                TaskCompletionSource<string> passwordResponse = new TaskCompletionSource<string>();
               
                List<PasswordFinder> passwordFinders = new List<PasswordFinder>();
                for (int i = 1; i <= int.Parse(threadBox.Text); i++)
                {
                    var passwordFinder = new PasswordFinder(Input.Text, passwordResponse, passwordQueue,i);
                    passwordFinders.Add(passwordFinder);
                    taskList.Add(passwordFinder.Run());
                }

                bool Handled = true;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                try
                {
                    Task<string> passwordTasks = passwordResponse.Task;
                    passwordTasks.Wait();
                    
                    Output.Content = "Password is " + passwordTasks.Result;
                    ShowLog(passwordFinders, int.Parse(threadBox.Text));
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

        private void ShowLog(List<PasswordFinder> passwordFinders, int numOfThreads)
        {
            
            for (int i = 0; i < passwordFinders.Max(s => s.log.Count); i++)
            {
                for (int j = 0; j < passwordFinders.Count; j++)
                {
                    listBox.Items.Add(passwordFinders[j].log[i]);
                }
            }

            //Ebalo si e maykata
            string lastMsg = "";
            foreach (PasswordFinder passwordFinder in passwordFinders)
            {
                lastMsg = passwordFinder.log.FirstOrDefault(s => s.Contains("found"));
                if (lastMsg != null)
                {
                    break;
                }
            }  
            listBox.Items.Add(lastMsg);
            listBox.SelectedItem = lastMsg;
        }
    }
    
}
