namespace BackendAPI.Models
{
    // --- DTOs (Data Transfer Objects) for API requests ---

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class PhoneRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class VerifyTokenRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RoleUpdateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class IssueRequest
    {
        public int ItemId { get; set; }
        public string IssuedTo { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string RequestedBy { get; set; } = "user";
    }

    public class ItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ModelNo { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StorageLocL1 { get; set; } = string.Empty;
        public string StorageLocL2 { get; set; } = string.Empty;
        public DateTime? WarrantyExpiration { get; set; }
    }

    public class PurchaseDto
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string PurchaseDate { get; set; } = string.Empty;
    }

    public class ItemWithPurchaseRequest
    {
        public ItemDto Item { get; set; } = new();
        public PurchaseDto Purchase { get; set; } = new();
    }

    public class UpdateItemWithPurchaseRequest
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ModelNo { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StorageLocL1 { get; set; } = string.Empty;
        public string StorageLocL2 { get; set; } = string.Empty;
        public DateTime? WarrantyExpiration { get; set; }
        public decimal? PurchasePrice { get; set; }
        public int? PurchaseQuantity { get; set; }
        public string? PurchaseDate { get; set; }
    }

    public class DeletePurchasesRequest
    {
        public int ItemId { get; set; }
        public List<int> OrderIds { get; set; } = new();
    }
}
