using System.Collections.Concurrent;

namespace BruteForceZipPasswordCracker
{
    // Password queue class
    class PasswordCollection : BlockingCollection<string>
    {
        public const int DEFAULT_MAX_SIZE = 1024;

        public PasswordCollection(int cMaximumSize = DEFAULT_MAX_SIZE) :
            base(new ConcurrentQueue<string>(), cMaximumSize)
        { }
    }
}
