using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using FeedbackService.Models;
using System;
using SharedLibrary;
using Microsoft.Extensions.Caching.Memory;
using FeedbackService.Data;
using Microsoft.EntityFrameworkCore;

namespace FeedbackService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
       private readonly IMemoryCache _memoryCache;
        private readonly ApplicationDbContext _context;
        public FeedbackController( IMemoryCache memoryCache, ApplicationDbContext context)
        {
           
            _memoryCache = memoryCache;
            _context = context;
        }

        [HttpGet("GetAllFeedback")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _context.Feedbacks.ToListAsync();
            return Ok(feedbacks);  // Wrap List in Ok() to return IActionResult
        }


        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetFeedbackByIdAsync(long id)
        {
            var f= await _context.Feedbacks.FindAsync(id);
            return Ok(f);
        }

        [HttpGet("TargetFeedback/{targetId}")]
        public async Task<IActionResult> GetFeedbackByTargetIdAsync(string targetId)
        {
            var f= await _context.Feedbacks
                .Where(f => f.TargetID == targetId)
                .ToListAsync();
            return Ok(f);
        }

        [HttpGet("SenderFeedback/{senderId}")]
        public async Task<IActionResult> GetFeedbackBySenderIdAsync(string senderId)
        {
           var f=  await _context.Feedbacks
                .Where(f => f.SenderID == senderId)
                .ToListAsync();
            return Ok(f);
        }

      
        [HttpPost("PostFeedback")]
        public async Task<IActionResult> PostFeedback([FromBody] FeedbackDTO feedback)
        {
            if (feedback == null)
                return BadRequest("Invalid feedback data");
            //if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            //{
            //    return BadRequest("No user found in cache.");
            //}
            var Feedback = new Feedback { 
            TargetID=feedback.TargetID,
            SenderID=feedback.SenderID,
            Comment=feedback.Comment,
            Rating=feedback.Rating,

            };


            Feedback.CreatedDate = DateTime.UtcNow;
            Feedback.ModifiedDate = DateTime.UtcNow;
            Feedback.Status = "Active";

            _context.Feedbacks.Add(Feedback);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Feedback created successfully", data = feedback });
        }

        [HttpPut("UpdateFeedback/{id}")]
        public async Task<IActionResult> UpdateFeedback(long id, [FromBody] FeedbackUpdateDTO feedback)
        {
            var existingFeedback = await _context.Feedbacks.FindAsync(id);
            if (existingFeedback == null) return null;

            existingFeedback.Comment = feedback.Comment;
            existingFeedback.Rating = feedback.Rating;
            existingFeedback.ModifiedDate = DateTime.UtcNow;
           existingFeedback.SenderID = feedback.SenderID;
            existingFeedback.TargetID=feedback.TargetID;

            _context.Feedbacks.Update(existingFeedback);
            await _context.SaveChangesAsync();
            if (existingFeedback == null)
                return BadRequest("Feedback not found");

            return Ok(new { message = "Feedback updated successfully", data = existingFeedback });
        }

        [HttpDelete("DeletedFeedback/{id}")]
        public async Task<IActionResult> DeleteFeedback(long id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return BadRequest("kuch galat hoa ha beta");
            feedback.Status = "Blocked";
            _context.Feedbacks.Update(feedback);

             await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback deleted successfully" });
        }
    }
}
