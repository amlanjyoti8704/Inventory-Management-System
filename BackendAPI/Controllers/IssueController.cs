using Microsoft.AspNetCore.Mvc;
using BackendAPI.Models;
using BackendAPI.Services;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// Handles the full issue request lifecycle:
    /// create, list, approve, decline, delete, return request/approve/reject.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : ControllerBase
    {
        private readonly IssueService _issueService;

        public IssueController(IssueService issueService)
        {
            _issueService = issueService;
        }

        // GET api/issue/items — Item list for the issue form dropdown
        [HttpGet("items")]
        public async Task<IActionResult> GetItems()
        {
            var items = await _issueService.GetItemsForIssueAsync();
            return Ok(items);
        }

        // POST api/issue — Create a new issue request (status=pending)
        [HttpPost]
        public async Task<IActionResult> CreateIssueRequest([FromBody] IssueRequest request)
        {
            try
            {
                await _issueService.CreateRequestAsync(request);
                return Ok(new { message = "Request submitted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET api/issue?requested_by= — List all issues, optionally filtered
        [HttpGet]
        public async Task<IActionResult> GetIssuedItems([FromQuery] string? requested_by)
        {
            var results = await _issueService.GetAllAsync(requested_by);
            return Ok(results);
        }

        // PUT api/issue/decline/{id} — Decline an issue request
        [HttpPut("decline/{id}")]
        public async Task<IActionResult> DeclineIssue(int id)
        {
            try
            {
                var success = await _issueService.DeclineAsync(id);
                return success
                    ? Ok(new { message = "Request declined." })
                    : NotFound(new { error = "Request not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT api/issue/approve/{id} — Approve request and deduct stock
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveIssue(int id)
        {
            try
            {
                var (success, error) = await _issueService.ApproveAsync(id);
                if (!success)
                    return error == "Not enough stock."
                        ? BadRequest(new { error })
                        : NotFound(new { error });

                return Ok(new { message = "Request approved and stock updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE api/issue/{id} — Delete an issue record
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssueRecord(int id)
        {
            try
            {
                var success = await _issueService.DeleteAsync(id);
                return success
                    ? Ok(new { message = "Issue record deleted successfully." })
                    : NotFound(new { error = "Issue record not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT api/issue/request-return/{id} — User requests to return items
        [HttpPut("request-return/{id}")]
        public async Task<IActionResult> RequestReturn(int id)
        {
            try
            {
                var success = await _issueService.RequestReturnAsync(id);
                return success
                    ? Ok(new { message = "Return request sent to admin." })
                    : BadRequest(new { error = "Return request failed. Either invalid ID or not in approved state." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT api/issue/approve-return/{id} — Admin approves return, stock restored
        [HttpPut("approve-return/{id}")]
        public async Task<IActionResult> ApproveReturn(int id)
        {
            try
            {
                var (success, error) = await _issueService.ApproveReturnAsync(id);
                return success
                    ? Ok(new { message = "Return approved and stock updated." })
                    : NotFound(new { error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT api/issue/reject-return/{id} — Admin rejects return request
        [HttpPut("reject-return/{id}")]
        public async Task<IActionResult> RejectReturn(int id)
        {
            try
            {
                var success = await _issueService.RejectReturnAsync(id);
                return success
                    ? Ok(new { message = "Return request rejected." })
                    : BadRequest(new { error = "Reject failed. Invalid request or not in 'requested' state." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET api/issue/pending-requests — Get all items needing admin attention
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var results = await _issueService.GetPendingRequestsAsync();
            return Ok(results);
        }
    }
}