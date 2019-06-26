namespace PackageUpdater
{
    using System.IO;

    public class GitPullOrigin : AbstractCliTask
    {
        public GitPullOrigin(DirectoryInfo directory)
            : base("git.exe", "pull origin", directory)
        {
        }
    }
}