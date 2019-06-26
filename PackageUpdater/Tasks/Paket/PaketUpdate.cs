namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class PaketUpdate : AbstractProcess
    {
        public PaketUpdate(Repository repository, FileInfo paketExe, string packageId, string group)
            : base(
                paketExe.FullName,
                $"update" + (string.IsNullOrWhiteSpace(packageId) ? string.Empty : $" {packageId}") + (string.IsNullOrWhiteSpace(group) ? string.Empty : $" --group {group}"),
                repository.Directory)
        {
            this.PackageId = packageId;
            this.Group = group;
        }

        public string PackageId { get; }

        public string Group { get; }

        public static bool TryCreate(Repository repository, string packageId, string group, out PaketUpdate update)
        {
            if (repository.Directory.EnumerateFiles("paket.dependencies").FirstOrDefault() is FileInfo dependencies &&
                repository.Directory.EnumerateFiles("paket.lock").Any() &&
                repository.Directory.EnumerateDirectories(".paket").FirstOrDefault() is DirectoryInfo paketDir &&
                paketDir.EnumerateFiles("paket.exe").FirstOrDefault() is FileInfo paketExe)
            {
                if (string.IsNullOrWhiteSpace(group) && string.IsNullOrWhiteSpace(packageId))
                {
                    update = new PaketUpdate(repository, paketExe, null, null);
                    return true;
                }

                var deps = File.ReadAllText(dependencies.FullName);
                if (!string.IsNullOrWhiteSpace(group) &&
                    !Regex.IsMatch(deps, $"^ *group {group} *\r?\n", RegexOptions.Multiline))
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

                update = new PaketUpdate(repository, paketExe, packageId, group);
                return true;
            }

            update = null;
            return false;
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