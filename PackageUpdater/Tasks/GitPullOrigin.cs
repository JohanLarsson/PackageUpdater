namespace PackageUpdater
{
    using System.IO;
    using System.Threading.Tasks;

    public class GitPullOrigin : AbstractProcess
    {
        public GitPullOrigin(DirectoryInfo directory)
            : base("git.exe", "pull origin", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Success = true;
        }
    }
}