using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BruteForceZipPasswordCracker
{
    internal class PasswordGenerator
    {
        private StringBuilder passwordBuilder;
        private BlockingCollection<string> passwordQueue;
        private CancellationToken cancellationToken;

        public PasswordGenerator(BlockingCollection<string> passwordQueue, string initialPassword = "")
        {
            this.passwordBuilder = new StringBuilder(initialPassword);
            this.passwordQueue = passwordQueue;
        }

        public async Task Run()
        {
            await Task.Run(() => this.AddNextPasswordToQueue());
        }

        private void AddNextPasswordToQueue()
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string nextPassword = NextPassword();
                    passwordQueue.Add(nextPassword, cancellationToken);
                }
            }
            catch(OperationCanceledException)
            { }
        }
        private string NextPassword()
        {
            string currentPassword = passwordBuilder.ToString();

            GenerateNextPassword();

            return currentPassword;
        }

        private void GenerateNextPassword()
        {
            if(passwordBuilder.Length == 0)
            {
                AddNewDigit();
            }
            else
            {
                int counter = 0;
                bool stop = false;

                while (!stop)
                {
                    passwordBuilder[counter] = (char)NextDigit(int.Parse(passwordBuilder[counter].ToString()));

                    if(passwordBuilder[counter] == '0')
                    {
                        counter++;

                        if(counter == passwordBuilder.Length)
                        {
                            AddNewDigit();
                            stop = true;
                        }
                    }
                    else
                    {
                        stop = true;
                    }
                }
            }
        }

        private void AddNewDigit()
        {
            passwordBuilder.Append('0');
        }

        private int NextDigit(int currentDigit)
        {
            int nextDigit;
            
            if(currentDigit == 9)
            {
                nextDigit = 0;
            }
            else
            {
                nextDigit = ++currentDigit;
            }

            return nextDigit;
        }

    }
}
