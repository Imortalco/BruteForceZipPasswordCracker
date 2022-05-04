using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;

namespace BruteForceZipPasswordCracker
{
    internal class PasswordFinder
    {
        class ZipFileNotExistentException : Exception
        { }

        class ZipFileNotPasswordProtectedException : Exception
        { }
        public string FoundPassword { get; set; }

        private string fileName;
        private TaskCompletionSource<string> passwordResponse;
        private BlockingCollection<string> passwordQueue;
        
        public PasswordFinder(string fileName,TaskCompletionSource<string> passwordResponse, BlockingCollection<string> passwordQueue)
        {
            this.fileName = fileName;
            this.passwordResponse = passwordResponse;
            this.passwordQueue = passwordQueue;
        }

        public async Task Run()
        {
            await Task.Run(() => this.FindPassword());
        }

        private void FindPassword()
        {

            if (!File.Exists(fileName))
            {
                passwordResponse.TrySetException(new ZipFileNotExistentException());
            }
            else if(!IsPasswordProtected())
            {
                passwordResponse.TrySetException(new ZipFileNotPasswordProtectedException());
            }

            while (!passwordResponse.Task.IsCompleted)
            {
                string currentPassword = passwordQueue.Take();

                bool passwordFound = TryPassword(currentPassword);

                if (passwordFound == true)
                {
                    passwordResponse.SetResult(currentPassword);
                    break;
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
            catch (Exception) { }
            return passwordFound;
        }
    }
}
