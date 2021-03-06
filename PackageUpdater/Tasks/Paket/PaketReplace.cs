namespace PackageUpdater
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Wpf.Reactive;

    public sealed class PaketReplace : AbstractTask, IDisposable
    {
        private readonly FileInfo dependencies;
        private readonly string oldDependency;
        private readonly string oldReference;
        private readonly string newDependency;
        private readonly string newReference;
        private bool disposed;

        private PaketReplace(FileInfo dependencies, string oldDependency, string oldReference, string newDependency, string newReference)
        {
            this.dependencies = dependencies;
            this.oldDependency = oldDependency;
            this.oldReference = oldReference;
            this.newDependency = newDependency;
            this.newReference = newReference;
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
            this.DisplayText = $"s/{oldDependency}/{newDependency}";
        }

        public override AsyncCommand StartCommand { get; }

        public override string DisplayText { get; }

        public static bool TryCreate(Repository repository, string? oldDependency, string? newDependency, [NotNullWhen(true)] out PaketReplace? update)
        {
            if (TryGetDependency(oldDependency, out oldDependency) &&
                TryGetReference(oldDependency, out var oldReference) &&
                TryGetDependency(newDependency, out newDependency) &&
                TryGetReference(newDependency, out var newReference) &&
                repository.TryGetPaketFiles(out var dependencies, out _, out _))
            {
                var dependenciesText = File.ReadAllText(dependencies.FullName);
                if (!dependenciesText.Contains(oldDependency, StringComparison.Ordinal) ||
                    dependenciesText.Contains(newDependency, StringComparison.Ordinal))
                {
                    update = null;
                    return false;
                }

                update = new PaketReplace(dependencies, oldDependency, oldReference, newDependency, newReference);
                return true;
            }

            update = null;
            return false;

            static bool TryGetDependency(string? candidate, out string dependency)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    dependency = null!;
                    return false;
                }

                if (candidate.IndexOf(' ', StringComparison.Ordinal) < 0)
                {
                    dependency = $"nuget {candidate}";
                }
                else if(candidate.IndexOf(' ', StringComparison.Ordinal) == candidate.IndexOf(" prerelease", StringComparison.Ordinal))
                {
                    dependency = $"nuget {candidate}";
                }
                else
                {
                    dependency = candidate;
                }

                return true;
            }

            static bool TryGetReference(string dependency, out string reference)
            {
                var parts = dependency.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    reference = parts[1];
                    return true;
                }

                reference = null!;
                return false;
            }
        }

        public override async Task RunAsync()
        {
            this.ThrowIfDisposed();
            this.Status = Status.Running;
            await ReplaceAsync(this.dependencies.FullName, this.oldDependency, this.newDependency).ConfigureAwait(false);

            foreach (var subDir in this.dependencies.Directory!.EnumerateDirectories())
            {
                if (subDir.EnumerateFiles("paket.references").FirstOrDefault() is { } references)
                {
                    await ReplaceAsync(references.FullName, this.oldReference, this.newReference).ConfigureAwait(false);
                }
            }

            this.Status = Status.Success;

            static async Task ReplaceAsync(string fileName, string old, string @new)
            {
                var text = await ReadAsync(fileName).ConfigureAwait(false);
                await using var writer = new StreamWriter(fileName, append: false);
                await writer.WriteAsync(text.Replace(old, @new, StringComparison.Ordinal)).ConfigureAwait(false);
            }

            static async Task<string> ReadAsync(string fileName)
            {
                using var reader = File.OpenText(fileName);
                return await reader.ReadToEndAsync().ConfigureAwait(false);
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
