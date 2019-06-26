namespace PackageUpdater
{
    using System.IO;

    public class DotnetRestore : AbstractProcess
    {
        public DotnetRestore(DirectoryInfo directory)
            : base("dotnet.exe", "restore", directory)
        {
        }
    }
}
