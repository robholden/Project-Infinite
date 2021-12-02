namespace Library.Core;

public class SiteException : Exception
{
    private readonly object[] _values;

    public SiteException(ErrorCode errorCode)
        : base($"Error Code: { errorCode } ({ errorCode })")
    {
        ErrorCode = errorCode;
    }

    public SiteException(ErrorCode errorCode, params object[] values)
        : this(errorCode)
    {
        _values = values;
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

    public ErrorCode ErrorCode { get; internal set; } = ErrorCode.Default;

    public ErrorCodeDto ToDto() => new(ErrorCode, _values);
}