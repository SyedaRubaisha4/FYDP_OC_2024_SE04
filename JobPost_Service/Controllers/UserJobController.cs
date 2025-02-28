using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using JobPost_Service.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserJobController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;  

        public UserJobController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        [HttpPost("CreateUserjob")]
        public async Task <ActionResult<UserJob>> CreateUserJob(UserJobCreateDTO UserJobCreateDTO)
        {

            if (UserJobCreateDTO.JobId != null && UserJobCreateDTO.UserId != null)
            {
                var UserJob = new UserJob { 
                    UserId = UserJobCreateDTO.UserId,
                    JobId=UserJobCreateDTO.JobId,
                    CreatedDate=DateTime.Now,
                    Status=Status.Active.ToString(),
                    ModifiedDate=null
                };

                _context.UserJob.Add(UserJob);
                _context.SaveChanges();
                return Ok(UserJob);

            }
            else
            {
                return BadRequest("User id or Job is null");
            }
        }
        [HttpPut("UpdateUserjob/{id}")]
        public async Task<ActionResult<UserJob>> UpdateUserJob(UserJobUpdateDTO UserJobUpdateDTO)
        {

            var userjob = await _context.UserJob.FindAsync(UserJobUpdateDTO.Id);
            if (UserJobUpdateDTO.Id!=null)
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
            var allusers= _context.UserJob.Where(x=> x.Status==Status.Active.ToString()).ToList();
            return Ok(allusers);
        }
    }
}
