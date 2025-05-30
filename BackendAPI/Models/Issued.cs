public class Issued
{
    public int IssueId { get; set; } // Matches issue_id (Foreign Key)
    public int ItemId { get; set; } // Matches item_id (Foreign Key)
    public DateTime IssueDate { get; set; } // Matches issue_date
}