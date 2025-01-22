using Proto.IdentityV1;
using Xunit;

namespace FunctionalTests.Grpc;

public class CreateAccountUseCaseShould : FunctionalTestBase
{
    public CreateAccountUseCaseShould(SubstituteWebApplicationFactory webApplicationFactory) 
        : base(webApplicationFactory){}
    
}