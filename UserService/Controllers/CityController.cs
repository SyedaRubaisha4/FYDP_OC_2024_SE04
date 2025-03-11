using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Helper;
using UserService.Models;
using UserService.Models.DTOs;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/City
        [HttpGet("GetAllCity")]
        public async Task<ActionResult<IEnumerable<City>>> GetCity()
        {
            return await _context.City.ToListAsync();
        }

        // GET: api/City/5
        [HttpGet("GetCityById/{id}")]
        public async Task<ActionResult<City>> GetCityById(long id)
        {
            var city = await _context.City.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            return city;
        }

        [HttpPut("UpdateCity/{id}")]
        public async Task<IActionResult> UpdateCity(long id, CityCreateDto city)
        {
            if (id == null)
            {
                return BadRequest();
            }


            var City = await _context.City.FindAsync(id);
            if(City== null)
            {
                return BadRequest("No City found ");
            }
            else
            {
                City.Name=city.Name;
                City.ModifiedDate = DateTime.Now;
                 _context.City.Update(City);
                return Ok(City);
            }
        }

        [HttpPost("AddCity")]
        public async Task<ActionResult<City>> AddCity(CityCreateDto City)
        {
            if(City!=null)
            {

                var city = new City
                {
                    Name = City.Name,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = null,
                    Status = Status.Active.ToString(),
                   
                };
                _context.City.Add(city);
                await _context.SaveChangesAsync();
                return city;       
            }
            else
            {
                return BadRequest(" City Name null");
            }            
        }

        // DELETE: api/City/5
        [HttpDelete("DeleteCity/{id}")]
        public async Task<IActionResult> DeleteCity(long id)
        {
            var city = await _context.City.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            city.Status=Status.Blocked.ToString();
            city.ModifiedDate=DateTime.UtcNow;

            _context.City.Remove(city);
            await _context.SaveChangesAsync();

            return Ok("City deleted successfully");
        }

        private bool CityExists(long id)
        {
            return _context.City.Any(e => e.Id == id);
        }
    }
}
