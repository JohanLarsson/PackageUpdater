namespace PackageUpdater
{
    using System.IO;
    using System.Threading.Tasks;

    public class GitAssertEmptyDiff : AbstractProcess
    {
        public GitAssertEmptyDiff(DirectoryInfo directory)
            : base("git.exe", "diff", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Success = this.Datas.Count == 0;
        }
    }
}