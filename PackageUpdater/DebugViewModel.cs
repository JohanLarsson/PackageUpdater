namespace PackageUpdater
{
    using System.IO;

    public class DebugViewModel
    {
        public static DirectoryInfo WorkingDirectory { get; } = new DirectoryInfo("C:\\Git\\_GuOrg\\Gu.Inject");

        public DotnetRestore DotnetRestore { get; } = new DotnetRestore(WorkingDirectory);

        public GitAssertEmptyDiff AssertEmptyDiff { get; } = new GitAssertEmptyDiff(WorkingDirectory);
    }
}