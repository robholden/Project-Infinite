namespace Library.Core;

public class ApiSiteException : Exception
{
    public ApiSiteException(int statusCode, SiteException siteException)
    {
        StatusCode = statusCode;
        SiteException = siteException;
    }

    public ApiSiteException() : base()
    {
    }

    public ApiSiteException(string message) : base(message)
    {
    }

    public ApiSiteException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SiteException SiteException { get; set; }

    public int StatusCode { get; set; }
}