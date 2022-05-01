using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;

namespace BruteForceZipPasswordCracker
{
    internal class PasswordFinder
    {
        //Exception in case Zip file does not exist
        class ZipFileNotExistentException : Exception
        { }

        // Exception in case Zip file is not password protected
        class ZipFileNotPasswordProtectedException : Exception
        { }
        public string FoundPassword;

        private string fileName;
        private TaskCompletionSource<string> passwordResponse;
        private BlockingCollection<string> passwordQueue;
        
        public PasswordFinder(string fileName,TaskCompletionSource<string> passwordResponse)
        {
            this.fileName = fileName;
            this.passwordResponse = passwordResponse;
        }

        public async Task Run()
        {
            await Task.Run(() => this.FindPassword());
        }

        private void FindPassword()
        {
            if (!File.Exists(fileName))
            {
                passwordResponse.TrySetException(new ZipFileNotExistentException);
            }
            else if(!IsPasswordProtected())
            {
                passwordResponse.TrySetException(new ZipFileNotPasswordProtectedException);
            }

            while (!passwordResponse.Task.IsCompleted)
            {
                string currentPassword = passwordQueue.Take();

                bool passwordFound = TryPassword(currentPassword);

                if (passwordFound)
                {
                    passwordResponse.SetResult(currentPassword);
                }
            }
        }

        private bool IsPasswordProtected()
        {
            return !ZipFile.CheckZipPassword(fileName, "");
        }

        public bool TryPassword(string password)
        {
            bool passwordFound = false;

            try
            {
                passwordFound = ZipFile.CheckZipPassword(fileName,password);
                if (passwordFound == true)
                {
                    this.FoundPassword = password;
                }
            }
            catch { }

            return passwordFound;
        }
    }
}
