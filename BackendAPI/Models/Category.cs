namespace BackendAPI.Models
{
    /// <summary>
    /// Maps to the "category" table in PostgreSQL.
    /// </summary>
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Threshold { get; set; }
    }
}