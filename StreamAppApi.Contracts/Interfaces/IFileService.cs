using StreamAppApi.Contracts.Commands.FileCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IFileService
{
    Task<Dictionary<string, FileDto>> SaveFiles(FilesAddCommand fileAddCommand, CancellationToken cancellationToken);
}