using Microsoft.AspNetCore.Hosting;

using StreamAppApi.Contracts.Commands.FileCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.Bll;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public FileService(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
    }

    public async Task<List<FileDto>> SaveFiles(
        FilesAddCommand filesAddCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        List<FileDto> result = new();

        foreach (var file in filesAddCommand.files)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not provided or empty.");
            }

            var uploadsFolder = _hostingEnvironment.ContentRootPath
                + @"\wwwroot\uploads\"
                + (filesAddCommand.folder ?? "default");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = file.FileName.Replace(" ", "_");
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            result.Add(new(Path.Combine(uploadsFolder, fileName).Replace("\\", "/"), fileName));
        }

        return result;
    }
}