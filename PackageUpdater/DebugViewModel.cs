namespace PackageUpdater
{
    using System.IO;

    public sealed class DebugViewModel : System.IDisposable
    {
        private bool disposed;

        public static DirectoryInfo WorkingDirectory { get; } = new DirectoryInfo("C:\\Git\\_GuOrg\\Gu.Inject");

        public DotnetRestore DotnetRestore { get; } = new DotnetRestore(WorkingDirectory);

        public GitAssertEmptyDiff AssertEmptyDiff { get; } = new GitAssertEmptyDiff(WorkingDirectory);

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.DotnetRestore?.Dispose();
            this.AssertEmptyDiff?.Dispose();
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