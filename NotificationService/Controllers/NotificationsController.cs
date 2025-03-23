using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NotificationService.Data;
//ing NotificationService.Migrations;
using NotificationService.Models;
using SharedLibrary;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        private readonly IRequestClient<GetUserByIdRequest> _userClient;


        public NotificationsController(IMemoryCache memoryCache, ApplicationDbContext context, IRequestClient<GetUserByIdRequest> userClient)
        {
            _memoryCache = memoryCache;
            _context = context;
            _userClient = userClient;
        }

      
        [HttpGet("GetAllNotifications")]
        public async Task<ActionResult<IEnumerable<GetAllJobAcceptedNotificationDTO>>> GetNotification()
        {
            // ✅ User cache se get karna
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }

            // ✅ Job Notifications fetch karna
            var notifications = await _context.AcceptedJobNotifcation
                .Where(x => x.ReceiverId == user.Id)
                .ToListAsync();

            List<GetAllJobAcceptedNotificationDTO> responseList = new List<GetAllJobAcceptedNotificationDTO>();

            foreach (var notification in notifications)
            {
                // ✅ User Service ko Request bhejna
                var response = await _userClient.GetResponse<GetUserByIdResponse>(new GetUserByIdRequest { UserId = notification.SenderId });

                var userResponse = response.Message; // Ye User ka data hoga

                // ✅ Notification DTO me Data Populate karna
                var dto = new GetAllJobAcceptedNotificationDTO
                {
                  
                   JobStatus=notification.JobStatus,
                    IsSee=notification.IsSee,
                    CreatedDate=notification.CreatedDate,
                    SenderName = userResponse?.Name 
                };

                responseList.Add(dto);
            }

            return Ok(responseList);
        }


        [HttpGet("GetUnreadCount/{userId}")]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            try
            {
                int unreadCount = await _context.AcceptedJobNotifcation
                    .Where(n => n.ReceiverId == userId && !n.IsSee)
                    .CountAsync();

                return Ok(unreadCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error fetching unread count: " + ex.Message);
            }
        }
        [HttpPost("MarkAsRead/{userId}")]
        public async Task<IActionResult> MarkAsRead(string userId)
        {
            try
            {
                var unreadNotifications = await _context.AcceptedJobNotifcation
                    .Where(n => n.ReceiverId == userId && !n.IsSee)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsSee = true; // Mark as read
                }

                await _context.SaveChangesAsync();
                return Ok("Notifications marked as read.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error updating notifications: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AcceptedJobNotifcation>> GetNotification(long id)
        {
            var AcceptedJobNotifcation = await _context.AcceptedJobNotifcation.FindAsync(id);

            if (AcceptedJobNotifcation == null)
            {
                return NotFound();
            }

            return AcceptedJobNotifcation;
        }

        // PUT: api/Notifications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification(long id, AcceptedJobNotifcation notification)
        {
            if (id != notification.Id)
            {
                return BadRequest();
            }

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Notifications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AcceptedJobNotifcation>> PostNotification(AcceptedJobNotifcation notification)
        {
            _context.AcceptedJobNotifcation.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(long id)
        {
            var notification = await _context.AcceptedJobNotifcation.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.AcceptedJobNotifcation.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificationExists(long id)
        {
            return _context.AcceptedJobNotifcation.Any(e => e.Id == id);
        }
    }
}
