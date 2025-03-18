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

            var userJobs = await _context.UserJob
                .Where(x => x.UserId == UserId && x.Status == Status.Active.ToString())
                .Select(x => new
                {
                    x.JobId,
                    x.CreatedDate
                })
                .ToListAsync();

            var userJobList = new List<JobPostDto1>();

            foreach (var userJob in userJobs)
            {
                var Job = await _context.JobPosts
                    .Where(x => x.Id == userJob.JobId && x.Status == Status.Active.ToString())
                    .FirstOrDefaultAsync();

                if (Job != null)
                {
                    var jobPostDTO = new JobPostDto1
                    {
                        JobType = Job.JobType,
                        WorkplaceType = Job.WorkplaceType,
                        Skills = Job.Skills,
                        CompanyName = Job.CompanyName,
                        Name = Job.Name,
                        Description = Job.Description,
                        Location = Job.Location,
                        Address = Job.Address,
                        PhoneNumber = Job.PhoneNumber,
                        Email = Job.Email,
                        Experience = Job.Experience,
                        MinSalary = Job.MinSalary,
                        MaxSalary = Job.MaxSalary,
                        Status = Job.Status,
                        DatePosted = Job.DatePosted,
                        UserId = Job.UserId,
                        CategoryId = Job.CategoryId,
                        Timing = Job.Timing,
                        Type = Job.Type,
                        JobsStatus = await _context.UserJob
                            .Where(x => x.UserId == UserId && x.JobId == userJob.JobId)
                            .Select(x => x.JobsStatus)
                            .FirstOrDefaultAsync()
                    };

                    // ✅ Assign AppliedDate after object initialization
                    jobPostDTO.AppliedDate = GetRelativeTime(userJob.CreatedDate ?? DateTime.UtcNow);

                    userJobList.Add(jobPostDTO);
                }
            }

            return Ok(userJobList);
        }

        // ✅ Function to calculate relative time
        private string GetRelativeTime(DateTime appliedAt)
        {
            TimeSpan timeDiff = DateTime.UtcNow - appliedAt;

            if (timeDiff.TotalSeconds < 60)
                return $"{(int)timeDiff.TotalSeconds} seconds ago";
            if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes} minutes ago";
            if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours} hours ago";
            if (timeDiff.TotalDays < 30)
                return $"{(int)timeDiff.TotalDays} days ago";
            if (timeDiff.TotalDays < 365)
                return $"{(int)(timeDiff.TotalDays / 30)} months ago";

            return $"{(int)(timeDiff.TotalDays / 365)} years ago";
        }

        [HttpGet("GetUserAppliedService")]
        public async Task<IActionResult> GetUserAppliedService(string UserId)
        {
            if (UserId == null)
            {
                return BadRequest("User not found ");
            }

            var userServices = await _context.UserService
                .Where(x => x.UserId == UserId && x.Status == Status.Active.ToString())
                .ToListAsync();  // List<UserService> mil raha hai, jo ki ServiceId ke saath CreatedDate bhi rakhta hai.

            var userServiceList = new List<JobServiceDTO1>();

            foreach (var userService in userServices) // userServices ek list hai jo `UserService` ka data rakhta hai
            {
                var service = await _context.ServicePosts
                    .Where(x => x.Id == userService.ServiceId && x.Status == Status.Active.ToString())
                    .FirstOrDefaultAsync();

                if (service != null)
                {
                    var jobServiceDTO = new JobServiceDTO1
                    {
                        Name = service.Name,
                        Description = service.Description,
                        Location = service.Location,
                        Address = service.Address,
                        PhoneNumber = service.PhoneNumber,
                        Email = service.Email,
                        Experience = service.Experience,
                        MinSalary = service.MinSalary,
                        MaxSalary = service.MaxSalary,
                        Status = service.Status,
                        DatePosted = service.DatePosted,
                        UserId = service.UserId,
                        CategoryId = service.CategoryId,
                        Timing = service.Timing,
                        Type = service.Type,
                        PreferredDate = service.PreferredDate,
                        UrgencyLevel = service.UrgencyLevel
                    };

                    jobServiceDTO.ServiceStatus = userService.ServiceStatus;
                    jobServiceDTO.AppliedDate = GetRelativeTime(userService.CreatedDate ?? DateTime.UtcNow);

                    userServiceList.Add(jobServiceDTO);
                }
            }
            return Ok(userServiceList);
        }

    }

}
