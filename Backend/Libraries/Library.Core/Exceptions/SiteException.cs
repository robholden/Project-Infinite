using System.Net;

namespace Library.Core;

public class SiteException : Exception
{
    private readonly object[] _values;

    public SiteException(ErrorCode errorCode)
        : base($"Error Code: {errorCode} ({errorCode})")
    {
        ErrorCode = errorCode;
    }

    public SiteException(ErrorCode errorCode, params object[] values)
        : this(errorCode)
    {
        _values = values;
    }

    public SiteException(ErrorCode errorCode, HttpStatusCode statusCode, params object[] values)
        : this(errorCode, values)
    {
        StatusCode = (int)statusCode;
    }

    public SiteException(string message) : base(message)
    {
    }

    public SiteException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private SiteException()
    {
    }

    public int? StatusCode { get; internal set; }

    public ErrorCode ErrorCode { get; internal set; } = ErrorCode.Default;

    public ErrorCodeDto ToDto() => new(ErrorCode, _values);
}