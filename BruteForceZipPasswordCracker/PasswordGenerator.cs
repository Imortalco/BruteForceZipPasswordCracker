using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BruteForceZipPasswordCracker
{
    internal class PasswordGenerator
    {
        private StringBuilder passwordBuilder;
        private BlockingCollection<string> passwordQueue;
        private CancellationToken cancellationToken;

        public PasswordGenerator(BlockingCollection<string> passwordQueue, CancellationToken cancellationToken,string initialPassword = "")
        {
            this.passwordBuilder = new StringBuilder(initialPassword);
            this.passwordQueue = passwordQueue;
            this.cancellationToken = cancellationToken;  
        }

        public async Task Run()
        {
            await Task.Run(() => this.AddNextPasswordToQueue(),cancellationToken);
        }

        private void AddNextPasswordToQueue()
        {
            try
            {
                while (true)
                {
                    string nextPassword = NextPassword();
                    
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        passwordQueue.Add(nextPassword, cancellationToken);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException){ }             
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
                    passwordBuilder[counter] = NextDigit(passwordBuilder[counter]);

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
            passwordBuilder.Append("0");
        }

        private char NextDigit(char currentChar)
        {
            int currentDigit = int.Parse(currentChar.ToString());
            int nextDigit;
            
            if(currentDigit == 9)
            {
                nextDigit = 0;
            }
            else
            {
                nextDigit = ++currentDigit;
            }

            return char.Parse(nextDigit.ToString());
        }

    }
}
