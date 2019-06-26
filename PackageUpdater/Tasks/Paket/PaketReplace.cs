namespace PackageUpdater
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Wpf.Reactive;

    public sealed class PaketReplace : AbstractTask, System.IDisposable
    {
        private readonly FileInfo dependencies;
        private readonly string oldPackageId;
        private readonly string newPackageId;
        private bool disposed;

        private PaketReplace(FileInfo dependencies, string oldPackageId, string newPackageId)
        {
            this.dependencies = dependencies;
            this.oldPackageId = oldPackageId;
            this.newPackageId = newPackageId;
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
            this.DisplayText = $"s/{oldPackageId}/{newPackageId}";
        }

        public override AsyncCommand StartCommand { get; }

        public override string DisplayText { get; }

        public static bool TryCreate(Repository repository, string oldPackageId, string newPackageId, out PaketReplace update)
        {
            if (!string.IsNullOrWhiteSpace(oldPackageId) &&
                !string.IsNullOrWhiteSpace(newPackageId) &&
                repository.TryGetPaketFiles(out var dependencies, out _, out _))
            {
                var deps = File.ReadAllText(dependencies.FullName);
                if (!deps.Contains($"nuget {oldPackageId}") ||
                    deps.Contains($"nuget {newPackageId}"))
                {
                    update = null;
                    return false;
                }

                update = new PaketReplace(dependencies, oldPackageId, newPackageId);
                return true;
            }

            update = null;
            return false;
        }

        public override async Task RunAsync()
        {
            this.ThrowIfDisposed();
            this.Status = Status.Running;
            await ReplaceAsync(this.dependencies.FullName);

            foreach (var subDir in this.dependencies.Directory.EnumerateDirectories())
            {
                if (subDir.EnumerateFiles("paket.references").FirstOrDefault() is FileInfo references)
                {
                    await ReplaceAsync(references.FullName).ConfigureAwait(false);
                }
            }

            this.Status = Status.Success;

            async Task ReplaceAsync(string fileName)
            {
                var text = await ReadAsync(fileName).ConfigureAwait(false);
                using (var writer = new StreamWriter(fileName, false))
                {
                    await writer.WriteAsync(text.Replace(this.oldPackageId, this.newPackageId));
                }
            }

            async Task<string> ReadAsync(string fileName)
            {
                using (var reader = File.OpenText(fileName))
                {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.StartCommand.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new System.ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}