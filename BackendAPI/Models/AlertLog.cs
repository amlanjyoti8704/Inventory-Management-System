namespace BackendAPI.Models
{
    /// <summary>
    /// Maps to the "alert_log" table in PostgreSQL.
    /// </summary>
    public class AlertLog
    {
        public int LogId { get; set; }
        public int? ItemId { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int CurrentQuantity { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; }
    }
}
