namespace PackageUpdater
{
    using System.IO;

    public class PaketInstall : AbstractCliProcess
    {
        private PaketInstall(DirectoryInfo repository, FileInfo paketExe)
            : base(paketExe.FullName," install", repository)
        {
        }

        public static bool TryCreate(Repository repository, out PaketInstall result)
        {
            if (repository.TryGetPaketFiles(out _, out _, out var paketExe))
            {
                result = new PaketInstall(repository.Directory, paketExe);
                return true;
            }

            result = null;
            return false;
        }
    }
}