namespace PackageUpdater
{
    using System.IO;
    using System.Linq;

    public class Repository
    {
        public Repository(FileInfo sln, FileInfo dependencies, FileInfo lockFile, FileInfo paketExe)
        {
            this.Sln = sln;
            this.Dependencies = dependencies;
            this.LockFile = lockFile;
            this.PaketExe = paketExe;
        }

        public FileInfo Sln { get; }

        public FileInfo Dependencies { get; }

        public FileInfo LockFile { get; }

        public FileInfo PaketExe { get; }

        public DirectoryInfo RootDirectory => this.Sln.Directory;

        public static bool TryCreate(string directory, out Repository repository)
        {
            if (Directory.EnumerateFiles(directory, "*.sln").FirstOrDefault() is string sln &&
                Directory.EnumerateFiles(directory, "paket.dependencies").FirstOrDefault() is string dependencies &&
                Directory.EnumerateFiles(directory, "paket.lock").FirstOrDefault() is string lockFile &&
                Directory.EnumerateDirectories(directory, ".paket").FirstOrDefault() is string paketDir &&
                Directory.EnumerateFiles(paketDir,"paket.exe").FirstOrDefault() is string paketExe)
            {
                repository = new Repository(new FileInfo(sln), new FileInfo(dependencies), new FileInfo(lockFile), new FileInfo(paketExe));
                return true;
            }

            repository = null;
            return false;
        }
    }
}