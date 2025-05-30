public class AlertLog
{
    public int LogId { get; set; } // Matches log_id (Primary Key)
    public int ItemId { get; set; } // Matches item_id
    public int CurrentQuantity { get; set; } // Matches current_quantity
    public string AlertMessage { get; set; } // Matches alert_message
    public DateTime AlertTime { get; set; } // Matches alert_time (Default CURRENT_TIMESTAMP)
}