namespace PackageUpdater
{
    using System.IO;
    using System.Threading.Tasks;
    using Gu.Wpf.Reactive;

    public sealed class PaketReplace : AbstractProcess, System.IDisposable
    {
        private bool disposed;

        public PaketReplace(FileInfo dependencies, string oldPackageId, string newPackageId, FileInfo paketExe)
        {
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
            this.DisplayText = $"s/{oldPackageId}/{newPackageId}";
        }

        public override AsyncCommand StartCommand { get; }

        public override string DisplayText { get; }

        public static bool TryCreate(Repository repository, string oldPackageId, string newPackageId, out PaketReplace update)
        {
            if (!string.IsNullOrWhiteSpace(oldPackageId) &&
                !string.IsNullOrWhiteSpace(newPackageId) &&
                repository.TryGetPaketFiles(out var dependencies, out _, out var paketExe))
            {
                var deps = File.ReadAllText(dependencies.FullName);
                if (!deps.Contains($"nuget {oldPackageId}") ||
                    deps.Contains($"nuget {newPackageId}"))
                {
                    update = null;
                    return false;
                }

                update = new PaketReplace(dependencies, oldPackageId, newPackageId, paketExe);
                return true;
            }

            update = null;
            return false;
        }

        public override Task RunAsync()
        {
            this.ThrowIfDisposed();
            throw new System.NotImplementedException();
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