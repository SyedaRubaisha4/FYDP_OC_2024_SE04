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
        [HttpGet("GetAlluser")]
        public async Task<IActionResult> GetAlluser()
        {
            var allusers = _context.UserJob.Where(x => x.Status == Status.Active.ToString()).ToList();
            return Ok(allusers);
        }
        [HttpGet("GetApplicantsByJob/{jobId}")]
        public async Task<IActionResult> GetApplicantsByJob(long jobId)
        {
            var userJobEntries = await _context.UserJob
                .Where(uj => uj.JobId == jobId && uj.Status == "Active")
                .Select(uj =>uj.UserId).ToListAsync();
            var userList = new List<PublishedUser>();
            foreach(var u in  userJobEntries)
            {
                PublishedUser getuser = await _userRequestProducer. RequestUserById(u);
                 userList.Add(getuser);
            }
            return Ok(userList);
        }
    }

}
