using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using JobPost_Service.Models.DTOs;
using JobPost_Service.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
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
            _userRequestProducer = UserRequestProducer;
        }

        [HttpPost("CreateUserjob")]
        public async Task<ActionResult<UserJob>> CreateUserJob(UserJobCreateDTO UserJobCreateDTO)
        {
           

            var UserJob = new UserJob
            {
                UserId = UserJobCreateDTO.UserId,
                JobId = UserJobCreateDTO.JobId,
                CreatedDate = DateTime.Now,
                Status = Status.Active.ToString(),
                JobsStatus = JobStatus.Applied.ToString()
            };
            if (UserJob.JobId != null)
            {
                _context.UserJob.Add(UserJob);
                await _context.SaveChangesAsync();
                return Ok(UserJob);
            }
            return BadRequest("JobId is null");
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

        public async Task<IActionResult> GetApplicantsByJob(long jobId, string UserId)
        {
            var userJobEntries = await _context.UserJob
                .Where(uj => uj.JobId == jobId && uj.Status == "Active")
                .Select(uj => uj.UserId)
                .Distinct()
                .ToListAsync();

            var userList = new List<PublishedUser>();
            foreach (var userId in userJobEntries)
            {
                PublishedUser getUser = await _userRequestProducer.RequestUserById(userId);
                getUser.IsApplied = _context.AcceptedJobApplication.Any(x => x.ApplicantId == userId && x.JobId == jobId && x.UserId == UserId);
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
                ServiceStatus = JobStatus.Applied.ToString()
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
        public async Task<IActionResult> GetApplicantsByService(long ServiceId, string UserId)
        {
            var userJobEntries = await _context.UserService
                .Where(uj => uj.ServiceId == ServiceId && uj.Status == "Active")
                .Select(uj => uj.UserId)
                .Distinct()
                .ToListAsync();

            var userList = new List<PublishedUser>();

            foreach (var userId in userJobEntries)
            {
                PublishedUser getUser = await _userRequestProducer.RequestUserById(userId);
                getUser.IsApplied = _context.AcceptedServiceApplication.Any(x => x.ApplicantId == userId && x.ServiceId == ServiceId && x.UserId == UserId);

                userList.Add(getUser);
            }

            return Ok(userList);
        }


        [HttpGet("GetUserJobCount")]
        public async Task<IActionResult> GetUserJobCount(string Id)
        {
            var count = await _context.UserJob.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(count);
        }
        [HttpGet("GetUserServiceCount")]
        public async Task<IActionResult> GetUserServiceCount(string Id)
        {
            var count = await _context.UserService.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(count);
        }

        [HttpPost("AcceptedJobApplication")]
        public async Task<IActionResult> AcceptedJobApplication(AcceptedJobApplicationDTO AcceptedJobApplicationDTO)
        {
            var acceptedJobApplication = new AcceptedJobApplication
            {
                UserId = AcceptedJobApplicationDTO.UserId,
                ApplicantId = AcceptedJobApplicationDTO.ApplicantId,
                JobId = AcceptedJobApplicationDTO.JobId,
                Status = Status.Active.ToString(),
                CreatedDate = DateTime.UtcNow,

            };

            _context.AcceptedJobApplication.Add(acceptedJobApplication);
            var job = _context.UserJob.Where(x => x.UserId == acceptedJobApplication.UserId && x.JobId == AcceptedJobApplicationDTO.JobId).FirstOrDefault();
            job.JobsStatus = AcceptedJobApplicationDTO.JobsStatus;
            _context.UserJob.Update(job);
            await _context.SaveChangesAsync();

            return Ok(acceptedJobApplication);
        }

        [HttpPost("AcceptedServiceApplication")]

        public async Task<IActionResult> AcceptedServiceApplication(AcceptedServiceApplicationDTO AcceptedServiceApplicationDTO)
        {
            var acceptedServiceApplication = new AcceptedServiceApplication
            {
                UserId = AcceptedServiceApplicationDTO.UserId,
                ApplicantId = AcceptedServiceApplicationDTO.ApplicantId,
                ServiceId = AcceptedServiceApplicationDTO.ServiceId,
                Status = Status.Active.ToString(),
                CreatedDate = DateTime.UtcNow,

            };
            _context.AcceptedServiceApplication.Add(acceptedServiceApplication);
            var service = await _context.UserService.Where(x => x.ServiceId == AcceptedServiceApplicationDTO.ServiceId && x.UserId == AcceptedServiceApplicationDTO.UserId).FirstOrDefaultAsync();
            service.ServiceStatus = AcceptedServiceApplicationDTO.ServiceStatus;
            _context.UserService.Update(service);
            await _context.SaveChangesAsync();
            return Ok(acceptedServiceApplication);
        }

        [HttpGet("GetUserAppliedJobs")]
        public async Task<IActionResult> GetUserAppliedJobs(string UserId)
        {
            if (UserId == null)
            {
                return BadRequest("User not found ");
            }
            var userJobs = _context.UserJob.Where(x => x.UserId == UserId && x.Status == Status.Active.ToString()).Select(x => x.JobId).ToList();
            var userJobList = new List<JobPost>();
            foreach (var job in userJobs)
            {
                var Job = await _context.JobPosts.Where(x => x.Id == job && x.Status == Status.Active.ToString()).FirstOrDefaultAsync();
                userJobList.Add(Job);
            }
            return Ok(userJobList);
        }
        [HttpGet("GetUserAppliedService")]
        public async Task<IActionResult> GetUserAppliedService(string UserId)
        {
            if (UserId == null)
            {
                return BadRequest("User not found ");
            }
            var userServices = _context.UserService.Where(x => x.UserId == UserId && x.Status == Status.Active.ToString()).Select(x => x.ServiceId).ToList();
            var userServiceList = new List<ServicePost>();
            foreach (var job in userServices)
            {
                var service = await _context.ServicePosts.Where(x => x.Id == job && x.Status == Status.Active.ToString()).FirstOrDefaultAsync();
                userServiceList.Add(service);
            }
            return Ok(userServiceList);
        }
    }

}
