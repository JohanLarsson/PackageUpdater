namespace PackageUpdater
{
    using System.IO;
    using System.Threading.Tasks;

    public class GitFetchOrigin : AbstractProcess
    {
        public GitFetchOrigin(DirectoryInfo directory)
            : base("git.exe", "fetch origin", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync();
        }
    }
}