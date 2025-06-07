public class IssueRecords
{
    public int IssueId { get; set; } // Matches issue_id (Primary Key)
    public string IssuedTo { get; set; } // Matches issued_to
    public string Department { get; set; } // Matches department
    public int Quantity { get; set; } // Matches quantity
    public string requested_by { get; set; }
    public string Status { get; set; } // Matches status
    public string ReturnStatus { get; set; } // Matches return_status
    public DateTime IssueDate { get; set; } // Matches issue_date
}