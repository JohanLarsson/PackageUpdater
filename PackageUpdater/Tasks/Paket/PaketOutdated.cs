﻿namespace PackageUpdater
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    public class PaketOutdated : AbstractCliTask
    {
        private PaketOutdated(DirectoryInfo repository, FileInfo paketExe)
            : base(paketExe.FullName, "outdated", repository)
        {
        }

        public static bool TryCreate(Repository repository, [NotNullWhen(true)] out PaketOutdated? result)
        {
            if (repository.TryGetPaketFiles(out _, out _, out var paketExe))
            {
                result = new PaketOutdated(repository.Directory, paketExe);
                return true;
            }

            result = null;
            return false;
        }
    }
}