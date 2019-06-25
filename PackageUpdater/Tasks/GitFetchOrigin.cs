namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class GitFetchOrigin : AbstractProcess
    {
        public GitFetchOrigin(DirectoryInfo directory)
            : base("git.exe", "fetch origin", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Success = this.Datas.Any(x => x.Data == "* master");
        }
    }
}