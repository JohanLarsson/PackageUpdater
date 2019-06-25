namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class GitAssertIsOnMaster : AbstractProcess
    {
        public GitAssertIsOnMaster(DirectoryInfo directory)
            : base("git.exe", "branch", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Status = this.Datas.Any(x => x.Data == "* master") ? Status.Success : Status.Error;
        }
    }
}