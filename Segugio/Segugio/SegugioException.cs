namespace Segugio;

public class SegugioException : Exception
{
    public SegugioException(string? message) : base(message)
    {
    }

    public SegugioException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}