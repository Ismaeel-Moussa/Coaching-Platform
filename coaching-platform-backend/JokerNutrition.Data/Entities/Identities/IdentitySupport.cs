using Microsoft.AspNetCore.Identity;

namespace JokerNutrition.Data.Entities.Identities;

public class UserRole : IdentityUserRole<int> { }
public class UserClaim : IdentityUserClaim<int> { }
public class UserLogin : IdentityUserLogin<int> { }
public class UserToken : IdentityUserToken<int> { }
public class RoleClaim : IdentityRoleClaim<int> { }
