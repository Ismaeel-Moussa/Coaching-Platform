using Microsoft.AspNetCore.Identity;

namespace JokerNutrition.Data.Entities.Identities;

public class Role : IdentityRole<int>
{
    public Role() { }
    public Role(string roleName) : base(roleName) { }
}
