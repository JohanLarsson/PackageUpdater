namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class PaketUpdate : AbstractProcess
    {
        public PaketUpdate(Repository repository, string packageId, string group)
            : base(
                repository.PaketExe.FullName,
                $"update" + (string.IsNullOrWhiteSpace(packageId) ? string.Empty : $" {packageId}") + (string.IsNullOrWhiteSpace(group) ? string.Empty : $" --group {group}"),
                repository.Dependencies.Directory)
        {
            this.PackageId = packageId;
            this.Group = group;
        }

        public string PackageId { get; }

        public string Group { get; }

        public static bool TryCreate(Repository repository, string packageId, string group, out PaketUpdate update)
        {
            if (string.IsNullOrWhiteSpace(group) && string.IsNullOrWhiteSpace(packageId))
            {
                update = new PaketUpdate(repository, null, null);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(group) &&
                !Regex.IsMatch(repository.DependenciesContent, $"^ *group {group} *\r?\n", RegexOptions.Multiline))
            {
                update = null;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(packageId) &&
                !repository.DependenciesContent.Contains($"nuget {packageId}"))
            {
                update = null;
                return false;
            }

            update = new PaketUpdate(repository, packageId, group);
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