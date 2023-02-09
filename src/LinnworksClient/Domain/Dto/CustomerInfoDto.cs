namespace LinnworksClient.Domain.Dto;

public sealed class CustomerInfoDto
{
    public CustomerInfoAddressDto Address { get; set; } = new();
}

public sealed class CustomerInfoAddressDto
{
    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Address1 { get; set; } = null!;

    public string Address2 { get; set; } = null!;

    public string Address3 { get; set; } = null!;

    public string Town { get; set; } = null!;

    public string Region { get; set; } = null!;

    public string PostCode { get; set; } = null!;

    public string Country { get; set; } = null!;
}
