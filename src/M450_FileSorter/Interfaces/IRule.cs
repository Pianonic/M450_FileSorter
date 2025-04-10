using M450_FileSorter.Models;

namespace M450_FileSorter.Interfaces
{
    public interface IRule
    {
        string Name { get; }
        string TargetTemplate { get; }
        bool Matches(FileMetadata fileMetadata);
        string GetTargetDirectory(FileMetadata fileMetadata);
    }
}
