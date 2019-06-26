namespace PackageUpdater
{
    using System.IO;

    public class GitCleanDxf : AbstractCliTask
    {
        public GitCleanDxf(DirectoryInfo directory)
            : base("git.exe", "clean -d -x -f", directory)
        {
        }
    }
}