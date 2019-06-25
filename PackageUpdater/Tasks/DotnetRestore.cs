namespace PackageUpdater
{
    using System.IO;
    using System.Threading.Tasks;

    public class DotnetRestore : AbstractProcess
    {
        public DotnetRestore(DirectoryInfo directory)
            : base("dotnet.exe", "restore", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Success = this.Errors.Count == 0;
        }
    }
}
