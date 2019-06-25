namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class PaketUpdate : AbstractProcess
    {

        public PaketUpdate(string group, string package, DirectoryInfo directory)
            : base(".paket\\paket.exe", $"update" + (string.IsNullOrWhiteSpace(package) ? string.Empty : $" {package}") + (string.IsNullOrWhiteSpace(group) ? string.Empty : $" --group {group}"), directory)
        {
            this.Group = group;
            this.Package = package;
        }

        public string Group { get; }

        public string Package { get; }

        public static bool TryCreate(Repository repository, string group, string packageId, out PaketUpdate update)
        {
            if (string.IsNullOrWhiteSpace(group) && string.IsNullOrWhiteSpace(packageId))
            {
                update = new PaketUpdate(null, null, repository.GitDirectory);
                return true;
            }

            var deps = File.ReadAllText(repository.Dependencies.FullName);

            if (!string.IsNullOrWhiteSpace(group) &&
                !deps.Contains($"group {group}"))
            {
                update = null;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(packageId) &&
                !deps.Contains($"nuget {packageId}"))
            {
                update = null;
                return false;
            }

            update = new PaketUpdate(packageId, group, repository.GitDirectory);
            return true;
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            if (this.Datas.Any(x => x.Data.Contains("paket.lock is already up-to-date")))
            {
                this.Status = Status.NoChange;
            }
            else if (this.Datas.Any(x => x.Data.Contains("Locked version resolution written to")))
            {
                this.Status = Status.Success;
            }
            else
            {
                this.Status = Status.Error;
            }
        }
    }
}