namespace LinnworksClient.Domain.Dto;

public sealed class OrderGeneralInfoDto
{
    public string ReferenceNum { get; set; } = null!;
    
    public string Source { get; set; } = null!;
    
    public string SubSource { get; set; } = null!;

    public int NumItems { get; set; }
}

