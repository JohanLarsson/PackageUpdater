namespace PackageUpdater
{
    using System.IO;

    public class GitCleanDxf : AbstractProcess
    {
        public GitCleanDxf(DirectoryInfo directory)
            : base("git.exe", "clean -d -x -f", directory)
        {
        }
    }
}