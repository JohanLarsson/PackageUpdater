﻿namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public sealed class Repository : INotifyPropertyChanged, System.IDisposable
    {
        private bool disposed;

        private Repository(DirectoryInfo directory)
        {
            this.Directory = directory;
            this.SolutionFiles = new ReadOnlyObservableCollection<FileInfo>(new ObservableCollection<FileInfo>(directory.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)));
            this.DotnetRestore = new DotnetRestore(directory);
            this.EmptyDiff = new GitAssertEmptyDiff(directory);
            this.IsOnMaster = new GitAssertIsOnMaster(directory);
            InitializeAsync();
            async void InitializeAsync()
            {
                try
                {
                    await this.EmptyDiff.RunAsync().ConfigureAwait(false);
                    await this.IsOnMaster.RunAsync().ConfigureAwait(false);
                }
                catch
                {
                    Debugger.Break();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DirectoryInfo Directory { get; }

        public ReadOnlyObservableCollection<FileInfo> SolutionFiles { get; }

        public DotnetRestore DotnetRestore { get; }

        public GitAssertEmptyDiff EmptyDiff { get; }

        public GitAssertIsOnMaster IsOnMaster { get; }

        public static bool TryCreate(string directory, [NotNullWhen(true)] out Repository? repository)
        {
            if (System.IO.Directory.EnumerateDirectories(directory, ".git").Any())
            {
                repository = new Repository(new DirectoryInfo(directory));
                return true;
            }

            repository = null;
            return false;
        }

        public bool TryGetPaketFiles([NotNullWhen(true)] out FileInfo? dependencies, [NotNullWhen(true)] out FileInfo? @lock, [NotNullWhen(true)] out FileInfo? paketExe)
        {
            if (this.Directory.EnumerateFiles("paket.dependencies").FirstOrDefault() is { } deps &&
                this.Directory.EnumerateDirectories(".paket").FirstOrDefault() is { } paketDir &&
                paketDir.EnumerateFiles("paket.exe").FirstOrDefault() is { } exe)
            {
                dependencies = deps;
                @lock = this.Directory.EnumerateFiles("paket.lock").FirstOrDefault();
                paketExe = exe;
                return true;
            }

            dependencies = null;
            @lock = null;
            paketExe = null;
            return false;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.DotnetRestore.Dispose();
            this.EmptyDiff.Dispose();
            this.IsOnMaster.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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