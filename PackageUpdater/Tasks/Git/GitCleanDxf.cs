namespace PackageUpdater
{
    using System.IO;

    public class GitCleanDxf : AbstractCliProcess
    {
        public GitCleanDxf(DirectoryInfo directory)
            : base("git.exe", "clean -d -x -f", directory)
        {
        }
    }
}