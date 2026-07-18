namespace JokerNutrition.Business.Common;

/// <summary>
/// Indicates that the caller is authenticated but is not allowed to access the requested resource.
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
