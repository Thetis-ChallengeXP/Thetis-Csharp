using ThetisService.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ThetisService.Implementations
{
    public class FileStorage : IFileStorage
    {
        public string BasePath { get; }

        public FileStorage(IHostEnvironment env)
        {
            BasePath = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(BasePath);
        }

        public async Task<string> SaveAsync(string fileName, byte[] content)
        {
            var full = Path.Combine(BasePath, fileName);
            await File.WriteAllBytesAsync(full, content);
            return full;
        }

        public Task<byte[]> ReadAsync(string fileName)
        {
            var full = Path.Combine(BasePath, fileName);
            return File.ReadAllBytesAsync(full);
        }

        public bool Exists(string fileName) => File.Exists(Path.Combine(BasePath, fileName));
    }
}
