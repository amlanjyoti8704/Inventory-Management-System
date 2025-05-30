public class IssueRecords
{
    public int IssueId { get; set; } // Matches issue_id (Primary Key)
    public string IssuedTo { get; set; } // Matches issued_to
    public string Department { get; set; } // Matches department
    public int Quantity { get; set; } // Matches quantity
    public string requested_by { get; set; }
}