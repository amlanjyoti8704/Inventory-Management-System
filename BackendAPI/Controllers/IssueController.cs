using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class IssueController : ControllerBase
{
    private readonly MongoDbContext _context;

    public IssueController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet("items")]
    public IActionResult GetItems()
    {
        var items = _context.ConsumableItems
            .Find(Builders<ConsumableItems>.Filter.Empty)
            .ToList();

        var result = items.Select(i => new
        {
            item_id = i.ItemId,
            item_name = i.Name,
            stock = i.Quantity
        }).ToList();

        return Ok(result);
    }

    // POST: Create a new issue request (status = 'pending'), no stock change here!
    [HttpPost]
    public IActionResult CreateIssueRequest([FromBody] IssueRequest request)
    {
        try
        {
            var issueId = _context.GetNextSequence("issue_records");

            var issue = new IssueRecords
            {
                IssueId = issueId,
                IssuedTo = request.issued_to,
                Department = request.department,
                Quantity = request.quantity,
                ItemId = request.item_id,
                IssueDate = DateTime.Now,
                Status = "pending",
                requested_by = request.requested_by ?? "user",
                ReturnStatus = "none"
            };

            _context.IssueRecords.InsertOne(issue);

            return Ok(new { message = "Request submitted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetIssuedItems([FromQuery] string? requested_by)
    {
        var filterBuilder = Builders<IssueRecords>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(requested_by))
            filter = filterBuilder.Eq(i => i.requested_by, requested_by);

        var issues = _context.IssueRecords
            .Find(filter)
            .ToList();

        // Get item names for the item_ids
        var itemIds = issues.Select(i => i.ItemId).Distinct().ToList();
        var items = _context.ConsumableItems
            .Find(Builders<ConsumableItems>.Filter.In(i => i.ItemId, itemIds))
            .ToList()
            .ToDictionary(i => i.ItemId, i => i.Name);

        var result = issues.Select(i => new
        {
            issue_id = i.IssueId,
            item_name = items.ContainsKey(i.ItemId) ? items[i.ItemId] : "Unknown",
            issued_to = i.IssuedTo,
            department = i.Department,
            quantity = i.Quantity,
            issue_date = i.IssueDate,
            status = i.Status,
            requested_by = i.requested_by,
            return_status = i.ReturnStatus
        }).ToList();

        return Ok(result);
    }

    [HttpPut("decline/{id}")]
    public IActionResult DeclineIssue(int id)
    {
        try
        {
            var filter = Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id);
            var exists = _context.IssueRecords.CountDocuments(filter) > 0;

            if (!exists)
                return NotFound(new { error = "Request not found." });

            var update = Builders<IssueRecords>.Update.Set(i => i.Status, "declined");
            _context.IssueRecords.UpdateOne(filter, update);

            return Ok(new { message = "Request declined." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PUT: Approve request and deduct stock
    [HttpPut("approve/{id}")]
    public IActionResult ApproveIssue(int id)
    {
        try
        {
            // Step 1: Get item_id and quantity for the given issue
            var issueFilter = Builders<IssueRecords>.Filter.And(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                Builders<IssueRecords>.Filter.Eq(i => i.Status, "pending")
            );

            var issue = _context.IssueRecords.Find(issueFilter).FirstOrDefault();

            if (issue == null)
                return NotFound(new { error = "Request not found or already processed." });

            // Step 2: Check stock
            var itemFilter = Builders<ConsumableItems>.Filter.Eq(i => i.ItemId, issue.ItemId);
            var item = _context.ConsumableItems.Find(itemFilter).FirstOrDefault();

            if (item == null || item.Quantity < issue.Quantity)
                return BadRequest(new { error = "Not enough stock." });

            // Step 3: Deduct stock
            var stockUpdate = Builders<ConsumableItems>.Update.Inc(i => i.Quantity, -issue.Quantity);
            _context.ConsumableItems.UpdateOne(itemFilter, stockUpdate);

            // Step 4: Update issue status
            var statusUpdate = Builders<IssueRecords>.Update.Set(i => i.Status, "approved");
            _context.IssueRecords.UpdateOne(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                statusUpdate);

            return Ok(new { message = "Request approved and stock updated." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteIssueRecord(int id)
    {
        try
        {
            var filter = Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id);
            var result = _context.IssueRecords.DeleteOne(filter);

            if (result.DeletedCount > 0)
                return Ok(new { message = "Issue record deleted successfully." });
            else
                return NotFound(new { error = "Issue record not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("request-return/{id}")]
    public IActionResult RequestReturn(int id)
    {
        try
        {
            var filter = Builders<IssueRecords>.Filter.And(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                Builders<IssueRecords>.Filter.Eq(i => i.Status, "approved")
            );

            var update = Builders<IssueRecords>.Update.Set(i => i.ReturnStatus, "requested");
            var result = _context.IssueRecords.UpdateOne(filter, update);

            if (result.ModifiedCount == 0)
                return BadRequest(new { error = "Return request failed. Either invalid ID or not in approved state." });

            return Ok(new { message = "Return request sent to admin." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("approve-return/{id}")]
    public IActionResult ApproveReturn(int id)
    {
        try
        {
            // Step 1: Get the issue record
            var issueFilter = Builders<IssueRecords>.Filter.And(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                Builders<IssueRecords>.Filter.Eq(i => i.Status, "approved"),
                Builders<IssueRecords>.Filter.Eq(i => i.ReturnStatus, "requested")
            );

            var issue = _context.IssueRecords.Find(issueFilter).FirstOrDefault();

            if (issue == null)
                return NotFound(new { error = "Record not found or already returned." });

            // Step 2: Update return_status
            var returnUpdate = Builders<IssueRecords>.Update.Set(i => i.ReturnStatus, "approved");
            _context.IssueRecords.UpdateOne(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                returnUpdate);

            // Step 3: Update stock in consumableItems
            var stockUpdate = Builders<ConsumableItems>.Update.Inc(i => i.Quantity, issue.Quantity);
            _context.ConsumableItems.UpdateOne(
                Builders<ConsumableItems>.Filter.Eq(i => i.ItemId, issue.ItemId),
                stockUpdate);

            return Ok(new { message = "Return approved and stock updated." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("reject-return/{id}")]
    public IActionResult RejectReturn(int id)
    {
        try
        {
            var filter = Builders<IssueRecords>.Filter.And(
                Builders<IssueRecords>.Filter.Eq(i => i.IssueId, id),
                Builders<IssueRecords>.Filter.Eq(i => i.ReturnStatus, "requested")
            );

            var update = Builders<IssueRecords>.Update.Set(i => i.ReturnStatus, "rejected");
            var result = _context.IssueRecords.UpdateOne(filter, update);

            if (result.ModifiedCount == 0)
                return BadRequest(new { error = "Reject failed. Invalid request or not in 'requested' state." });

            return Ok(new { message = "Return request rejected." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("pending-requests")]
    public IActionResult GetPendingRequests()
    {
        var filter = Builders<IssueRecords>.Filter.Or(
            Builders<IssueRecords>.Filter.In(i => i.Status, new[] { "pending", "requested" }),
            Builders<IssueRecords>.Filter.Eq(i => i.ReturnStatus, "requested")
        );

        var pendingIssues = _context.IssueRecords
            .Find(filter)
            .ToList();

        return Ok(pendingIssues);
    }
}

// Model (kept in-file as in original)
public class IssueRequest
{
    public int item_id { get; set; }
    public string issued_to { get; set; }
    public string department { get; set; }
    public int quantity { get; set; }
    public string requested_by { get; set; }
}