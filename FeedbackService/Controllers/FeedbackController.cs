using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using FeedbackService.Models;
using System;
using SharedLibrary;
using Microsoft.Extensions.Caching.Memory;
using FeedbackService.Data;
using Microsoft.EntityFrameworkCore;
using FeedbackService.RabbitMQ;

namespace FeedbackService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
       private readonly IMemoryCache _memoryCache;
        private readonly ApplicationDbContext _context;
        private readonly UserRequestProducer _userRequestProducer;
        public FeedbackController( IMemoryCache memoryCache, ApplicationDbContext context, UserRequestProducer UserRequestProducer)
        {
           
            _memoryCache = memoryCache;
            _context = context;
            _userRequestProducer = UserRequestProducer;
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
            var feedbacks = await _context.Feedbacks
                .Where(f => f.TargetID == targetId)
                .ToListAsync();

            var feedbackList = new List<object>();

            foreach (var feedback in feedbacks)
            {
                PublishedUser senderUser = await _userRequestProducer.RequestUserById(feedback.SenderID);

                feedbackList.Add(new
                {
                    feedback.Id,
                    feedback.Comment,
                    feedback.Rating,
                    feedback.Status,
                    feedback.CreatedDate,
                    feedback.ModifiedDate,
                    Sender = senderUser != null ? new { senderUser.Id, senderUser.Name } : null
                });
            }

            return Ok(feedbackList);
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
        [HttpGet("GetUserRating/{targetId}")]
        public async Task<IActionResult> GetUserRating(string targetId)
        {
            var feedbacks = await _context.Feedbacks
                .Where(f => f.TargetID == targetId)
                .ToListAsync();

            if (!feedbacks.Any())
                return Ok(new { message = "No ratings yet", averageRating = 0 });

            // Total rating sum
            double totalStars = (double)feedbacks.Sum(f => f.Rating);

            // Average rating
            double averageRating = totalStars / feedbacks.Count;

            return Ok(new { targetId, averageRating = Math.Round(averageRating, 1) });
        }



    }
}
