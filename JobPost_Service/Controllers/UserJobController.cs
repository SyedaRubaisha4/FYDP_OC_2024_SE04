using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using JobPost_Service.Models.DTOs;
using JobPost_Service.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserJobController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly UserRequestProducer _userRequestProducer;
        public UserJobController(ApplicationDbContext context, IMemoryCache memoryCache, UserRequestProducer UserRequestProducer)
        {
            _context = context;
            _memoryCache = memoryCache;
            _userRequestProducer= UserRequestProducer;
        }

        [HttpPost("CreateUserjob")]
        public async Task<ActionResult<UserJob>> CreateUserJob(UserJobCreateDTO UserJobCreateDTO)
        {
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }

            var UserJob = new UserJob
            {
                UserId = user?.Id ?? "123",
                JobId = UserJobCreateDTO.JobId,
                CreatedDate = DateTime.Now,
                Status = Status.Active.ToString(),
                ModifiedDate = null
            };
            if (UserJob.JobId != null) 
            {
                _context.UserJob.Add(UserJob);
                await _context.SaveChangesAsync();
                return Ok(UserJob);
            }
            return BadRequest("JobId is null");
        }
        [HttpPut("UpdateUserjob/{id}")]
        public async Task<ActionResult<UserJob>> UpdateUserJob(UserJobUpdateDTO UserJobUpdateDTO)
        {

            var userjob = await _context.UserJob.FindAsync(UserJobUpdateDTO.Id);
            if (UserJobUpdateDTO.Id != null)
            {


                userjob.UserId = UserJobUpdateDTO.UserId;
                userjob.JobId = UserJobUpdateDTO.JobId;
                //  userjob.CreatedDate = UserJobUpdateDTO.CreatedDate;

                userjob.ModifiedDate = DateTime.Now;

                _context.UserJob.Update(userjob);
                _context.SaveChanges();
                return Ok(userjob);

            }
            else
            {
                return BadRequest("UserJob id is null");
            }
        }
        [HttpDelete("DeleteUserJob/{id}")]
        public async Task<IActionResult> Deleteuser(long Id)
        {
            var userjob = await _context.UserJob.FindAsync(Id);
            if (Id != null)
            {
                userjob.Status = Status.Blocked.ToString();
                _context.UserJob.Update(userjob);
                _context.SaveChanges();
                return Ok(userjob);

            }
            else
            {
                return BadRequest("UserJob id is null");

            }
        }
       
        [HttpGet("GetApplicantsByJob/{jobId}")]
        
        public async Task<IActionResult> GetApplicantsByJob(long jobId)
        {
            var userJobEntries = await _context.UserJob
                .Where(uj => uj.JobId == jobId && uj.Status == "Active")
                .Select(uj => uj.UserId)
                .Distinct() // Remove duplicate UserIds
                .ToListAsync();

            var userList = new List<PublishedUser>();

            foreach (var userId in userJobEntries)
            {
                PublishedUser getUser = await _userRequestProducer.RequestUserById(userId);
                userList.Add(getUser);
            }

            return Ok(userList);
        }


        [HttpPost("CreateUserService")]
        public async Task<ActionResult<UserService>> CreateUserService(UserServiceCreateDTO UserServiceCreateDTO)
        {
            //if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            //{
            //    return BadRequest("No user found in cache.");
            //}

            var UserService = new UserService
            {
                UserId = UserServiceCreateDTO.UserId,
                ServiceId = UserServiceCreateDTO.ServiceId,
                CreatedDate = DateTime.Now,
                Status = Status.Active.ToString(),
                ModifiedDate = null
            };
            if (UserService.ServiceId != null)
            {
                _context.UserService.Add(UserService);
                await _context.SaveChangesAsync();
                return Ok(UserService);
            }
            return BadRequest("ServiceId is null");
        }
        [HttpGet("GetApplicantsByService/{ServiceId}")]
        public async Task<IActionResult> GetApplicantsByService(long ServiceId)
        {
            var userJobEntries = await _context.UserService
                .Where(uj => uj.ServiceId == ServiceId && uj.Status == "Active")
                .Select(uj => uj.UserId)
                .Distinct() // Duplicate UserIds remove kar raha hai
                .ToListAsync();

            var userList = new List<PublishedUser>();

            foreach (var userId in userJobEntries)
            {
                PublishedUser getUser = await _userRequestProducer.RequestUserById(userId);
                userList.Add(getUser);
            }

            return Ok(userList);
        }


        [HttpGet("GetUserJobCount")]
        public async Task<IActionResult> GetUserJobCount(string Id)
        {
            var count =await _context.UserJob.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(count);
        }
        [HttpGet("GetUserServiceCount")]
        public async Task<IActionResult> GetUserServiceCount(string Id)
        {
            var count = await _context.UserService.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(count);
        }
    }

}
