namespace Api.Application.Common.Exceptions;

public sealed class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message) { }
    public ExternalServiceException(string message, Exception inner) : base(message, inner) { }
}
