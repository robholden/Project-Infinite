using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class UpdateMobileRequest
{
    [DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }
}