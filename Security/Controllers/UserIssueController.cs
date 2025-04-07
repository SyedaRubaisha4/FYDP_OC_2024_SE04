using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Security.Data;
using Security.Models;
using Security.Models.DTOs;
using SharedLibrary;
using MassTransit;

namespace Security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserIssueController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRequestClient<GetUserByIdRequest> _userClient;



        public UserIssueController(ApplicationDbContext context, IRequestClient<GetUserByIdRequest> userClient)
        {
            _context = context;
            _userClient = userClient;
        }

      
        [HttpGet("getallissues")]
        public async Task<ActionResult<List<UserIssueList>>> GetUserIssue()
        {
            var issues =await _context.UserIssue.Where(x => x.Status == "Active").ToListAsync();
            var UserIssueList =new List<UserIssueList>();
            foreach ( var i in issues)
            {
                var UserIssues= new UserIssueList();
                var response1 = await _userClient.GetResponse<GetUserByIdResponse>(new GetUserByIdRequest { UserId = i.UserId});
                var response2  = await _userClient.GetResponse<GetUserByIdResponse>(new GetUserByIdRequest { UserId = i.ReportingUserId});
                UserIssues.ReportedUserName = response1.Message.Name;
                UserIssues.ReportingUserName=response2.Message.Name;
                UserIssues.Issue = i.Issue;
                UserIssues.CreatedDate = i.CreatedDate;
                UserIssues.Id=i.Id;
                UserIssues.Status=i.Status;
                UserIssues.IssueResolve=i.IssueResolve;
                UserIssueList.Add(UserIssues);
            }

            return UserIssueList;
        }

        // GET: api/UserIssue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserIssue>> GetUserIssue(long id)
        {
            var UserIssue = await _context.UserIssue.FindAsync(id);

            if (UserIssue == null)
            {
                return NotFound();
            }

            return UserIssue;
        }      
       
        [HttpPost("createuserissue")]
        public async Task<ActionResult<UserIssue>> PostUserIssue(UserIssueCreateDto issuedto)
        {

            var issue = new UserIssue { 
            UserId=issuedto.UserId,
            Issue=issuedto.Message,
            ReportingUserId=issuedto.ReportingUserId,
            CreatedDate=DateTime.UtcNow,
            IssueResolve=false,
            Status="Active",
            };
                 if(issuedto.Image!= null)
                 {
               
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CategoryImages");

                    Directory.CreateDirectory(uploadFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(issuedto.Image.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await issuedto.Image.CopyToAsync(fileStream);
                    }

                    issue.ImageName = $"{uniqueFileName}";

                _context.UserIssue.Add(issue);
            }
                else
                {
                 issue.ImageName = "";
            _context.UserIssue.Add(issue);
                }

            await _context.SaveChangesAsync();
            return Ok(issue);
          //  return CreatedAtAction("GetUserIssue", new { id = UserIssue.Id }, UserIssue);
        }

        // DELETE: api/UserIssue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserIssue(long id)
        {
            var UserIssue = await _context.UserIssue.FindAsync(id);
            if (UserIssue == null)
            {
                return NotFound();
            }
            UserIssue.Status = "Blocked";
            _context.UserIssue.Update(UserIssue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserIssueExists(long id)
        {
            return _context.UserIssue.Any(e => e.Id == id);
        }
    }
}
