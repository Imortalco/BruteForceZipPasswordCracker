using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public List<string> log { get; set; }

        private string fileName;
        private TaskCompletionSource<string> passwordResponse;
        private BlockingCollection<string> passwordQueue;
        private int currentThreadIndex;
        
       
        
        public PasswordFinder(string fileName,TaskCompletionSource<string> passwordResponse, BlockingCollection<string> passwordQueue,int index)
        {
            this.fileName = fileName;
            this.passwordResponse = passwordResponse;
            this.passwordQueue = passwordQueue;
            this.currentThreadIndex = index;
            this.log = new List<string>();
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
                log.Add("Thread " + currentThreadIndex + ": trying " + currentPassword);
                bool passwordFound = TryPassword(currentPassword);

                if (passwordFound == true)
                {
                    passwordResponse.SetResult(currentPassword);
                    
                    log.Add("Thread " + currentThreadIndex + ": found " + currentPassword);
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
