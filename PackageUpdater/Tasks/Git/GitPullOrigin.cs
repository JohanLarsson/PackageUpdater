namespace PackageUpdater
{
    using System.IO;

    public class GitPullOrigin : AbstractCliProcess
    {
        public GitPullOrigin(DirectoryInfo directory)
            : base("git.exe", "pull origin", directory)
        {
        }
    }
}