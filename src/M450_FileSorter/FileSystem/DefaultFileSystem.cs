using M450_FileSorter.Interfaces;
using M450_FileSorter.Models;

namespace M450_FileSorter.FileSystem
{
    public class DefaultFileSystem : IFileSystem
    {
        public IEnumerable<string> GetFiles(string directory)
        {
            try
            {
                return Directory.Exists(directory)
                    ? Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly)
                    : Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Cannot access directory {directory}: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        public FileMetadata? GetFileMetadata(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                var fileInfo = new FileInfo(filePath);
                return new FileMetadata(
                    fileInfo.FullName,
                    fileInfo.Name,
                    fileInfo.Length,
                    fileInfo.CreationTimeUtc,
                    fileInfo.LastWriteTimeUtc
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Cannot get metadata for {filePath}: {ex.Message}");
                return null;
            }
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(destinationPath))
                {
                    string dir = Path.GetDirectoryName(destinationPath) ?? ".";
                    string baseName = Path.GetFileNameWithoutExtension(destinationPath);
                    string ext = Path.GetExtension(destinationPath);
                    int counter = 1;
                    string newPath;
                    do
                    {
                        newPath = Path.Combine(dir, $"{baseName}_{counter}{ext}");
                        counter++;
                    } while (File.Exists(newPath));
                    destinationPath = newPath;
                    Console.WriteLine($"[Warning] Destination file exists. Renaming to: {Path.GetFileName(destinationPath)}");
                }

                Console.WriteLine($"Moving '{Path.GetFileName(sourcePath)}' to '{destinationPath}'");
                File.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to move {sourcePath} to {destinationPath}: {ex.Message}");
            }
        }

        public void CreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"Created directory: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to create directory {path}: {ex.Message}");
            }
        }

        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
    }
}
