namespace M450_FileSorter.Models
{
    public record FileMetadata(
        string FullPath,
        string FileName,
        long Size,
        DateTime CreationTime,
        DateTime LastWriteTime
    );
}
