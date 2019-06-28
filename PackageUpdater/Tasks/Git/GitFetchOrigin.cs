namespace PackageUpdater
{
    using System.IO;

    public class GitFetchOrigin : AbstractCliTask
    {
        public GitFetchOrigin(DirectoryInfo directory)
            : base("git.exe", "fetch origin", directory)
        {
        }
    }
}