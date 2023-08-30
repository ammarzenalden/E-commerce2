namespace E2.DTO;

public class PaginationParams
{
    private const int maxItemPerPage = 20;
    public int Page { get; set; } = 1;
    private int itemPerPage = 10 ;
    public int ItemPerPage
    {
        get => itemPerPage;
        set => itemPerPage = value > maxItemPerPage ? maxItemPerPage :value;
    }
}
