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

namespace Security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserIssueResolverController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserIssueResolverController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserIssueResolver
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserIssueResolve>>> GetuserIssueResolves()
        {
            var responses=await _context.userIssueResolves.Where(x=>x.Status=="Active").ToListAsync();
            return responses;
        }

        // GET: api/UserIssueResolver/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserIssueResolve>> GetUserIssueResolve(long id)
        {
            var userIssueResolve = await _context.userIssueResolves.FindAsync(id);

            if (userIssueResolve == null)
            {
                return NotFound("Response not found ;(");
            }

            return userIssueResolve;
        }

        // PUT: api/UserIssueResolver/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserIssueResolve(long id, UserIssueResolveDto userIssueResolve)
        {
          var response = await _context.userIssueResolves.Where(x=>x.Status=="Active" &&  x.Id==id).FirstOrDefaultAsync();
            if(response!=null)
            {
                response.UserIssueId = response.UserIssueId;
                response.Response=response.Response;
            }

            var issue = await _context.UserIssue.Where(x => x.Id == response.UserIssueId && x.Status == "Active").FirstOrDefaultAsync();
            issue.IssueResolve = true;
            _context.UserIssue.Update(issue);
            _context.userIssueResolves.Update(response);
            await _context.SaveChangesAsync();



            return Ok(new
            {
                message = "Response updated successfully",
                issue = issue,
                response = response
            });
        }

        // POST: api/UserIssueResolver
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("issueresponse")]
        public async Task<ActionResult<UserIssueResolve>> PostUserIssueResolve(UserIssueResolveDto userIssueResolvedto)
        {
            var UserIssueResolve = new UserIssueResolve { 
            UserIssueId=userIssueResolvedto.UserIssueId,
            Response=userIssueResolvedto.Response,
            CreatedDate=DateTime.UtcNow,
            Status="Active",
            }
            
            ;
            _context.userIssueResolves.Add(UserIssueResolve);


            var issue = await _context.UserIssue.Where(x=>x.Id==userIssueResolvedto.UserIssueId && x.Status=="Active").FirstOrDefaultAsync();
            issue.IssueResolve=true;
            _context.UserIssue.Update(issue);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Response added successfully",
                issue = issue,
                response=UserIssueResolve
            });

        }

        // DELETE: api/UserIssueResolver/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserIssueResolve(long id)
        {
            var userIssueResolve = await _context.userIssueResolves.FindAsync(id);
            if (userIssueResolve == null)
            {
                return NotFound();
            }
            userIssueResolve.Status = "Blocked";
            _context.userIssueResolves.Update(userIssueResolve);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully");
        }

        private bool UserIssueResolveExists(long id)
        {
            return _context.userIssueResolves.Any(e => e.Id == id);
        }
    }
}
