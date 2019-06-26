namespace PackageUpdater
{
    using System.IO;

    public class GitPullFastForwardOnly : AbstractCliTask
    {
        public GitPullFastForwardOnly(DirectoryInfo directory)
            : base("git.exe", "pull --ff-only", directory)
        {
        }
    }
}