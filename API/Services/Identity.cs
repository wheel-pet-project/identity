using Grpc.Core;

namespace API.Services;

public class Identity : API.Identity.IdentityBase
{
    private readonly ILogger<Identity> _logger;
    
    public Identity(ILogger<Identity> logger) => _logger = logger;

    public override Task<AuthorizeResp> Authorize(AuthorizeReq req, ServerCallContext context)
    {
        Guid id = Guid.NewGuid();
        for (var i = 0; i < 10000; i++)
        {
            id = Guid.NewGuid();
        }
        return Task.FromResult(new AuthorizeResp
        {
            CorTkn = "some-token",
            Role = Role.Admin,
            UserId = id.ToString()
        });
    }
}