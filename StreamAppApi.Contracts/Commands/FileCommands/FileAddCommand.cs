using Microsoft.AspNetCore.Http;

namespace StreamAppApi.Contracts.Commands.FileCommands;

public record FilesAddCommand(IFormFileCollection files, string? folder);