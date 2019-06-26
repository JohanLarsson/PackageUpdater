namespace PackageUpdater
{
    using System.IO;

    public class GitPullFastForwardOnly : AbstractProcess
    {
        public GitPullFastForwardOnly(DirectoryInfo directory)
            : base("git.exe", "pull --ff-only", directory)
        {
        }
    }
}