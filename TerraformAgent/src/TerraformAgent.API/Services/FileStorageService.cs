namespace TerraformAgent.Api.Services;

public class FileStorageService
{
    private readonly string _uploadPath;

    public FileStorageService()
    {
        _uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads");

        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string?> SaveFileAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream);

        return filePath;
    }
}
