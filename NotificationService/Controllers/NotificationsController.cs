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
using NuGet.Versioning;
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

      
        [HttpGet("GetAllNotification")]
        public async Task<ActionResult<IEnumerable<GetAllJobAcceptedNotificationDTO>>> GetNotification()
        {
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("you need to login ");
            }

            var notifications = await _context.AcceptedJobNotifcation
                .Where(x => x.ReceiverId == user.Id)
                .ToListAsync();

            List<GetAllJobAcceptedNotificationDTO> responseList = new List<GetAllJobAcceptedNotificationDTO>();

            foreach (var notification in notifications)
            {
                var response = await _userClient.GetResponse<GetUserByIdResponse>(new GetUserByIdRequest { UserId = notification.SenderId });

                var userResponse = response.Message;

                var dto = new GetAllJobAcceptedNotificationDTO
                {
                  
                   JobStatus=notification.JobStatus,
                    IsSee=notification.IsSee,
                    CreatedDate=notification.CreatedDate,
                    SenderName = userResponse?.Name ,
                    NotificationText=notification.NotificationText,
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
                    notification.IsSee = true; 
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

       

        }
    }
