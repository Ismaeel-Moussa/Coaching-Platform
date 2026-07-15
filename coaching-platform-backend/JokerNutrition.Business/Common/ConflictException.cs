namespace JokerNutrition.Business.Common;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
