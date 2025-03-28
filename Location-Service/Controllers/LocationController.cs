using Location_Service.Data;
using Location_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Location_Service.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Save Location (Ensure unique entry per UserId)
        [HttpPost("save")]
        public async Task<IActionResult> SaveLocation([FromBody] Location location)
        {
            if (location == null || string.IsNullOrEmpty(location.UserId))
                return BadRequest("Invalid data.");

            // Check if location for the user already exists
            var existingLocation = await _context.Locations.FirstOrDefaultAsync(l => l.UserId == location.UserId);
            if (existingLocation != null)
                return Conflict("User already has a saved location.");

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return Ok(location);
        }

        // ✅ Get Location by UserId
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetLocationByUserId(string userId)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.UserId == userId);
            if (location == null)
                return NotFound("Location not found.");

            return Ok(location);
        }

        // ✅ Get All Locations
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _context.Locations.ToListAsync();
            return Ok(locations);
        }

        // ✅ Update Location (Only if UserId matches)
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLocationById(int id, [FromBody] Location updatedLocation)
        {
            var existingLocation = await _context.Locations.FindAsync(id);
            if (existingLocation == null)
                return NotFound("Location not found.");

            // Ensure only the same user can update their location
            if (existingLocation.UserId != updatedLocation.UserId)
                return Unauthorized("You are not allowed to update this location.");

            existingLocation.Latitude = updatedLocation.Latitude;
            existingLocation.Longitude = updatedLocation.Longitude;

            await _context.SaveChangesAsync();
            return Ok(existingLocation);
        }

        // ✅ Delete Location (Only if UserId matches)
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLocationById(int id, [FromQuery] string userId)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return NotFound("Location not found.");

            // Ensure only the same user can delete their location
            if (location.UserId != userId)
                return Unauthorized("You are not allowed to delete this location.");

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok("Location deleted successfully.");
        }
    }
}
