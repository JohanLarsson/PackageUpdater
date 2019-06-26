namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class GitAssertEmptyDiff : AbstractCliProcess
    {
        public GitAssertEmptyDiff(DirectoryInfo directory)
            : base("git.exe", "diff", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Status = this.Data.Any() ? Status.Error : Status.Success;
        }
    }
}