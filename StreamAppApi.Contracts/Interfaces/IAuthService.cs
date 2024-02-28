using StreamAppApi.Contracts.Commands.AuthCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IAuthService
{
    Task<ResultAuthDto> RegisterUser(AuthRegisterCommand authRegisterCommand, CancellationToken cancellationToken);
    Task<ResultAuthDto> LoginUser(AuthLoginCommand authLoginCommandCommand, CancellationToken cancellationToken);
    Task<ResultAuthDto> GetNewTokens(AuthGetNewTokensCommand? getNewTokensCommand, 
        CancellationToken cancellationToken = default);
}