namespace PackageUpdater
{
    using System.IO;

    public class GitFetchOrigin : AbstractProcess
    {
        public GitFetchOrigin(DirectoryInfo directory)
            : base("git.exe", "fetch origin", directory)
        {
        }
    }
}