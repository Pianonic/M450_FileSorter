namespace M450_FileSorter.Interfaces
{
    public interface IConfigurationReader
    {
        Models.Configuration ReadConfiguration(string path);
    }
}
