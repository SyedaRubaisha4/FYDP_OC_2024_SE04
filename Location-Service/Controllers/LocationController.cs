using Location_Service.Data;
using Location_Service.Models;
using Location_Service.RabbitMQ;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using System.Threading.Tasks;

namespace Location_Service.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserRequestProducer _userRequestProducer;
        private readonly IPublishEndpoint _publishEndpoint;

        public LocationController(AppDbContext context, UserRequestProducer UserRequestProducer, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _userRequestProducer = UserRequestProducer;
            _publishEndpoint = publishEndpoint;
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

        [HttpPut("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationDto updatedLocation)
        {
            // Find existing location by UserId
            var existingLocation = await _context.Locations
                .FirstOrDefaultAsync(l => l.UserId == updatedLocation.UserId);

            if (existingLocation == null)
                return NotFound("Location not found for the user.");

            // Update latitude and longitude
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

        [HttpGet("GetUsersFromJob/{job}/{userId}")]
        public async Task<IActionResult> GetUsersFromJob(string job, string userId, [FromQuery] double MaxDistance = 5)
        {
            const double R = 6371; // Earth radius in km
         //   const double MaxDistance = 5; // 5km radius

            // 🔹 Get the current user’s location
            var currentUserLocation = await _context.Locations
                .Where(l => l.UserId == userId)
                .FirstOrDefaultAsync();

            if (currentUserLocation == null)
                return BadRequest("User location not found.");

            double userLatRad = ToRadians(currentUserLocation.Latitude);
            double userLonRad = ToRadians(currentUserLocation.Longitude);

            // 🔹 Fetch all locations from DB first
            var allUsers = await _context.Locations.ToListAsync();

            // 🔹 Perform distance calculation in C#
            var nearbyUsers = allUsers
     .Select(loc => new
     {
         loc.UserId,
         loc.Latitude,
         loc.Longitude,
         Distance = R * Math.Acos(
             Math.Min(1, Math.Max(-1,
                 Math.Cos(userLatRad) * Math.Cos(ToRadians(loc.Latitude)) *
                 Math.Cos(ToRadians(loc.Longitude) - userLonRad) +
                 Math.Sin(userLatRad) * Math.Sin(ToRadians(loc.Latitude))
             ))
         )
     })
     .Where(x => x.Distance <= MaxDistance && x.UserId != userId) // ✅ Apni ID ko exclude kiya
     .OrderBy(x => x.Distance)
     .ToList();


            if (!nearbyUsers.Any())
                return NotFound("No users found nearby.");

            var userList = new List<object>();

            // 🔹 Fetch user details from User Service using RabbitMQ
            foreach (var userLoc in nearbyUsers)
            {
                try
                {
                    PublishedUser userDetails = await _userRequestProducer.RequestUserById(userLoc.UserId);
                    if (userDetails != null && userDetails.Job == job)
                    {
                        userList.Add(new
                        {
                            userDetails.Id,
                            userDetails.Name,
                            userDetails.Job,
                            userLoc.Latitude,
                            userLoc.Longitude,
                            userLoc.Distance,
                            userDetails.PhoneNumber,
                            userDetails.UserImage
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error fetching user details: {ex.Message}");
                }
            }

            return Ok(userList);
        }

        // 🔹 Helper function for radians conversion
        private static double ToRadians(double angle) => Math.PI * angle / 180.0;
    }
}