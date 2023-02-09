namespace LinnworksClient.Domain.Dto;

public sealed class GenericPagedResultDto<T>
{
    public int PageNumber { get; set; }

    public int EntriesPerPage { get; set; }

    public long TotalEntries { get; set; }

    public int TotalPages { get; set; }

    public List<T> Data { get; set; } = new();
}
