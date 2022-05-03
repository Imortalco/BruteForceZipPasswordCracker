using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BruteForceZipPasswordCracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void CrackPassword()
        {
            using (CancellationTokenSource token_src = new CancellationTokenSource())
            {
                // Create task list
                List<Task> task_list = new List<Task>();

                // Create queues
                //var log_msg_queue = new LogMessageQueue();
                var password_queue = new PasswordCollection();

                // Create and add password producer
                var pwd_producer = new PasswordGenerator(password_queue, token_src.Token, "0");
                task_list.Add(pwd_producer.Run());

                Stopwatch stop_watch = new Stopwatch();
                stop_watch.Start();

                TaskCompletionSource<string> password_src = new TaskCompletionSource<string>();
                for (int i = 1; i <= 10; i++)
                {
                    var pwd_consumer = new PasswordFinder(Input.Text, password_src, password_queue);
                    task_list.Add(pwd_consumer.Run());
                }

                try
                {
                    // Wait for password to be found
                    Task<string> password_res = password_src.Task;
                    password_res.Wait();
                    // Log password
                    //LogPassword(password_res.Result);
                    Output.Text = password_res.Result;
                    token_src.Cancel();
                }
                catch (Exception ex)
                {
                    // Re-throw exception if cannot be handled
                    /*if (!HandleException(ex))
                    {
                        throw;
                    }*/
                }
                finally
                {
                    // Stop stopwatch
                    stop_watch.Stop();
                    // Cancel the other tasks (log consumer and password producer)
                    MessageBox.Show(stop_watch.Elapsed.ToString());
                    // Wait for all to finish
                    //Task.WaitAll(task_list.ToArray());
                    // Print elapsed time
                    //Console.WriteLine(String.Format("Elapsed time: {0}", stop_watch.Elapsed));
                }
            }
        }

        // Print header


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CrackPassword();
        }
    }

    
}
