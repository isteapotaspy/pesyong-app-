using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.Services;

public class AuthorizationService
{
    public AuthorizationService(AuthenticationService authenticationService)
    {
        var _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }
}
