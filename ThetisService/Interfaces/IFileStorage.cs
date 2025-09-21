namespace ThetisService.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(string fileName, byte[] content);
        Task<byte[]> ReadAsync(string fileName);
        bool Exists(string fileName);
        string BasePath { get; }
    }
}
