using System.Collections.Generic;

namespace BackendAPI.Models
{
    public class DeletePurchasesRequest
    {
        public int ItemId { get; set; }
        public List<int> OrderIds { get; set; }
    }
}
