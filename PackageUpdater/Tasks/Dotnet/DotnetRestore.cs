namespace PackageUpdater
{
    using System.IO;

    public class DotnetRestore : AbstractCliProcess
    {
        public DotnetRestore(DirectoryInfo directory)
            : base("dotnet.exe", "restore", directory)
        {
        }
    }
}
