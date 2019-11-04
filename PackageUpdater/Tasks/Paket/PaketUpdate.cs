namespace PackageUpdater
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class PaketUpdate : AbstractCliTask
    {
        private PaketUpdate(Repository repository, FileInfo paketExe, string? packageId, string? group)
            : base(
                paketExe.FullName,
                $"update" + (string.IsNullOrWhiteSpace(packageId) ? string.Empty : $" {packageId}") + (string.IsNullOrWhiteSpace(group) ? string.Empty : $" --group {group}"),
                repository.Directory)
        {
            this.PackageId = packageId;
            this.Group = group;
        }

        public string? PackageId { get; }

        public string? Group { get; }

        public static bool TryCreate(Repository repository, string? packageId, string? group, [NotNullWhen(true)] out PaketUpdate? result)
        {
            if (repository.TryGetPaketFiles(out var dependencies, out _, out var paketExe))
            {
                if (string.IsNullOrWhiteSpace(group) && string.IsNullOrWhiteSpace(packageId))
                {
                    result = new PaketUpdate(repository, paketExe, null, null);
                    return true;
                }

                var dependenciesText = File.ReadAllText(dependencies.FullName);
                if (!string.IsNullOrWhiteSpace(group) &&
                    !Regex.IsMatch(dependenciesText, $"^ *group {group} *\r?\n", RegexOptions.Multiline))
                {
                    result = null;
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(packageId) &&
                    !dependenciesText.Contains($"nuget {packageId}", StringComparison.Ordinal))
                {
                    result = null;
                    return false;
                }

                result = new PaketUpdate(repository, paketExe, packageId, group);
                return true;
            }

            result = null;
            return false;
        }

        public override async Task RunAsync()
        {
            await base.RunAsync().ConfigureAwait(false);
            if (this.Data.Any(x => x.Data.Contains("paket.lock is already up-to-date", StringComparison.Ordinal)))
            {
                this.Status = Status.NoChange;
            }
            else if (this.Data.Any(x => x.Data.Contains("Locked version resolution written to", StringComparison.Ordinal)))
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