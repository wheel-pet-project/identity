using System.Diagnostics;
using API.Transformers;
using Application.Application.Interfaces;
using Application.Application.UseCases.Account.Authenticate;
using Application.Application.UseCases.Account.Authorize;
using Application.Application.UseCases.Account.ConfirmEmail;
using Application.Application.UseCases.Account.Create;
using Application.Application.UseCases.Account.RefreshAccessToken;
using Grpc.Core;
using Proto.Identity;
using Status = Grpc.Core.Status;

namespace API.Services;

public class Identity(
    ILogger<Identity> logger,
    IUseCase<CreateAccountRequest, CreateAccountResponse> createQueryUseCase,
    IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse> authenticateUseCase,
    IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse> authorizeUseCase,
    IUseCase<ConfirmAccountEmailRequest, ConfirmAccountEmailResponse> confirmEmailUseCase,
    IUseCase<RefreshAccountAccessTokenRequest, RefreshAccountAccessTokenResponse> refreshAccessTokenUseCase)
    : Proto.Identity.Identity.IdentityBase
{
    private readonly ActivitySource _activitySource = new("Identity");

    public override async Task<CreateResponse> CreateAccount(
        CreateRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Create account request received")
            .SetTag("CorrelationId", request.CorId);
        logger.LogInformation("Creating account request received");

        var transformer = new RoleTransformer();
        var result = await createQueryUseCase.Execute(
            new CreateAccountRequest(Guid.Parse(request.CorId),
                transformer.FromRequest(request.Role),
                request.Email,
                request.Phone,
                request.Pass));

        return result.IsSuccess
            ? new CreateResponse { AccId = result.Value.AccountId.ToString() }
            : throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', result.Errors.Select(x => x.Message))));
    }

    public override async Task<AuthenticateResponse> Authenticate(
        AuthenticateRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Authenticate account request received")
            .SetTag("CorrelationId", request.CorId)
            .SetTag("Account email", request.Email);
        logger.LogInformation("Authenticating account request received");

        var result = await authenticateUseCase.Execute(
            new AuthenticateAccountRequest(request.Email,
                request.Pass));

        return result.IsSuccess
            ? new AuthenticateResponse 
                { Tkn = result.Value.AccessToken, RefreshTkn = result.Value.RefreshToken }
            : throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', result.Errors.Select(x => x.Message))));
    }

    public override async Task<AuthorizeResponse> Authorize(
        AuthorizeRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Authorize account request received")
            .SetTag("CorrelationId", request.CorId);
        logger.LogInformation("Authorizing account request received");

        var roleTransformer = new RoleTransformer();
        var statusTransformer = new StatusTransformer();

        var result = await authorizeUseCase.Execute(
            new AuthorizeAccountRequest(request.Tkn));

        return result.IsSuccess
            ? new AuthorizeResponse
            {
                AccId = result.Value.AccountId.ToString(), 
                Role = roleTransformer.ToResponse(result.Value.Role),
                Status = statusTransformer.ToResponse(result.Value.Status)
            }
            : throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', result.Errors.Select(x => x.Message))));
    }

    public override async Task<RefreshAccessTokenResponse> RefreshAccessToken(
        RefreshAccessTokenRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Refresh access token request received")
            .SetTag("CorrelationId", request.CorId);
        logger.LogInformation("Refresh access token request received");

        var result = await refreshAccessTokenUseCase.Execute(
            new RefreshAccountAccessTokenRequest(request.RefreshTkn));
        
        return result.IsSuccess
            ? new RefreshAccessTokenResponse
            {
                Tkn = result.Value.AccessToken,
                RefreshTkn = result.Value.RefreshToken
            }
            : throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', result.Errors.Select(x => x.Message))));
    }

    public override async Task<ConfirmEmailResponse> ConfirmEmail(ConfirmEmailRequest request,
        ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Confirm email request received")
            .SetTag("CorrelationId", request.CorId)
            .SetTag("AccountId", request.AccId);
        logger.LogInformation("Confirm email request received");

        var result = await confirmEmailUseCase.Execute(new ConfirmAccountEmailRequest(
            Guid.Parse(request.AccId), Guid.Parse(request.ConfirmationId)));

        return result.IsSuccess
            ? new ConfirmEmailResponse()
            : throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', result.Errors.Select(x => x.Message))));
    }
}