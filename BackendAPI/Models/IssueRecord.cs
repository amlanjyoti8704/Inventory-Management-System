namespace BackendAPI.Models
{
    /// <summary>
    /// Maps to the "issue_records" table in PostgreSQL.
    /// </summary>
    public class IssueRecord
    {
        public int IssueId { get; set; }
        public string IssuedTo { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string RequestedBy { get; set; } = "user";
        public string Status { get; set; } = "pending";
        public string ReturnStatus { get; set; } = "none";
        public DateTime IssueDate { get; set; }
        public int ItemId { get; set; }
    }
}
