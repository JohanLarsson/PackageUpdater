namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class GitCleanDxf : AbstractProcess
    {
        public GitCleanDxf(DirectoryInfo directory)
            : base("git.exe", "clean -dxf", directory)
        {
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            this.Success = this.Datas.Any(x => x.Data == "* master");
        }
    }
}