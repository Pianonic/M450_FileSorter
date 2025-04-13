using M450_FileSorter.Models;

namespace M450_FileSorter.Interfaces
{
    public interface IFileSystem
    {
        IEnumerable<string> GetFiles(string directory);
        FileMetadata? GetFileMetadata(string filePath);
        void MoveFile(string sourcePath, string destinationPath);
        void CreateDirectory(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
    }
}
