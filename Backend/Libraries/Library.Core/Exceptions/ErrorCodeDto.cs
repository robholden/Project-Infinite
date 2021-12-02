namespace Library.Core;

public class ErrorCodeDto
{
    public ErrorCodeDto()
    {
    }

    public ErrorCodeDto(ErrorCode errorCode, object[] values = null)
    {
        // Otherwise, get message from Enum descriptor
        var message = errorCode.Description();

        // If enum description is empty, do not return that code to the client
        if (string.IsNullOrEmpty(message))
        {
            errorCode = ErrorCode.Default;
        }

        // Format message with any provided values
        if (values?.Any() == true)
        {
            message = string.Format(message, values);
        }

        Message = message;
        Code = (int)errorCode;
        Params = values;
    }

    public string Message { get; set; }

    public int Code { get; set; }

    public object[] Params { get; set; }

    public void Throw()
    {
        throw new SiteException((ErrorCode)Code, Params);
    }
}