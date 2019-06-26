namespace PackageUpdater
{
    using System.IO;

    public class GitPullFastForwardOnly : AbstractCliProcess
    {
        public GitPullFastForwardOnly(DirectoryInfo directory)
            : base("git.exe", "pull --ff-only", directory)
        {
        }
    }
}