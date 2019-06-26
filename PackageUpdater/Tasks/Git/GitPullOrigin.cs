namespace PackageUpdater
{
    using System.IO;

    public class GitPullOrigin : AbstractProcess
    {
        public GitPullOrigin(DirectoryInfo directory)
            : base("git.exe", "pull origin", directory)
        {
        }
    }
}