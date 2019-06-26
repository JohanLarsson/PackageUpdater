namespace PackageUpdater
{
    using System.IO;

    public class DotnetRestore : AbstractCliTask
    {
        public DotnetRestore(DirectoryInfo directory)
            : base("dotnet.exe", "restore", directory)
        {
        }
    }
}
