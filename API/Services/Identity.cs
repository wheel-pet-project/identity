using System.Diagnostics;
using API.Transformers;
using Application.Application.Interfaces;
using Application.UseCases.Account.Authenticate;
using Application.UseCases.Account.Authorize;
using Application.UseCases.Account.Create;
using Grpc.Core;
using Proto.Identity;

namespace API.Services;

public class Identity(
    ILogger<Identity> logger,
    IUseCase<CreateAccountRequest, CreateAccountResponse> createQueryUseCase,
    IUseCase<AuthenticateAccountRequest, AuthenticateAccountResponse> authenticateUseCase,
    IUseCase<AuthorizeAccountRequest, AuthorizeAccountResponse> authorizeUseCase) 
    : Proto.Identity.Identity.IdentityBase
{
    readonly ActivitySource _activitySource = new("Identity");
    
    public override async Task<CreateResponse> CreateAccount(
        CreateRequest request, ServerCallContext context)
    {
        
        using var activity = _activitySource
            .StartActivity("Create account request received")
            .SetTag("CorrelationId", request.CorId);
        logger.LogInformation("Creating account request received: {request}", request);
        
        
        var transformer = new RoleTransformer();
        var result = await createQueryUseCase.Execute(
            new CreateAccountRequest(Guid.Parse(request.CorId), 
                transformer.FromRequest(request.Role), 
                request.Email, 
                request.Phone, 
                request.Pass));
    
        return new CreateResponse
        {
            AccId = result.AccountId.ToString()
        };
    }
    public override async Task<AuthenticateResponse> Authenticate(
        AuthenticateRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Authenticate account request received")
            .SetTag("CorrelationId", request.CorId)
            .SetTag("Account email", request.Email);
        logger.LogInformation("Authenticating account request received: {request}", request);
    
        var result = await authenticateUseCase.Execute(
            new AuthenticateAccountRequest(request.Email,
                request.Pass));
    
        return new AuthenticateResponse
        {
            Tkn = result.AccessToken
        };
    }
    
    public override async Task<AuthorizeResponse> Authorize(
        AuthorizeRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Authorize account request received")
            .SetTag("CorrelationId", request.CorId);
        logger.LogInformation("Authorizing account request received, correlationId: {request}",
            request.CorId);
    
        var roleTransformer = new RoleTransformer();
        var statusTransformer = new StatusTransformer();
        
        var result = await authorizeUseCase.Execute(
            new AuthorizeAccountRequest(request.Tkn));
    
        return new AuthorizeResponse
        {
            AccId = result.AccountId.ToString(),
            Role = roleTransformer.ToResponse(result.RoleId),
            Status = statusTransformer.ToResponse(result.StatusId)
        };
    }
}