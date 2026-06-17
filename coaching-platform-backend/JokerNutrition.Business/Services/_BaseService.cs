using System.Security.Claims;
using System.Security.Principal;
using JokerNutrition.Business.Security;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public abstract class _BaseService
{
    protected readonly IPrincipal _principal;
    protected readonly ILogger _logger;

    protected _BaseService(IPrincipal principal, ILogger logger)
    {
        _principal = principal;
        _logger = logger;
    }

    private AppPrincipal? _user;

    protected AppPrincipal LoggedInUser
    {
        get
        {
            if (_user != null) return _user;
            var principal = _principal as ClaimsPrincipal
                ?? throw new UnauthorizedAccessException("Unable to find logged in person information.");
            _user = AppClaimsTransformation.Transform(principal);
            return _user;
        }
    }
}
