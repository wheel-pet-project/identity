using System.Diagnostics;
using API.Transformers;
using Application.UseCases.CreateAccount;
using Application.UseCases.Interfaces;
using Grpc.Core;

namespace API.Services;

public class Identity(
    ILogger<Identity> logger,
    IUseCase<CreateAccountRequest, CreateAccountResponse> createUseCase) 
    : API.Identity.IdentityBase
{
    readonly ActivitySource _activitySource = new("Identity");

    public override async Task<CreateResponse> CreateAccount(
        CreateRequest request, ServerCallContext context)
    {
        using var activity = _activitySource
            .StartActivity("Create account request received")
            .SetTag("CorrelationId", request.CorId);
        
        var transformer = new RoleFromRequestTransformer();
        var result = await createUseCase.Execute(
            new CreateAccountRequest(Guid.Parse(request.CorId), 
                transformer.FromRequest(request.Role), 
                request.Email, 
                request.Phone, 
                request.Pass));

        return new CreateResponse
        {
            AccId = result.AccountId.ToString(),
            CorId = result.CorrelationId.ToString(),
            Err = null
        };
    }
}