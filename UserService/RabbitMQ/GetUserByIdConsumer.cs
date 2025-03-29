using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using UserService.Data;
using UserService.Helper;

namespace UserService.RabbitMQ
{
    public class GetUserByIdConsumer : IConsumer<GetUserByIdRequest>
    {
        private readonly ApplicationDbContext _context;

        public GetUserByIdConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<GetUserByIdRequest> context)
        {
            var user = await _context.Users
                .Where(x => x.Status == Status.Active.ToString() && x.Id == context.Message.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                await context.RespondAsync<GetUserByIdResponse>(null);
                return;
            }

            var response = new GetUserByIdResponse
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Experience = user.Experience,
                UserImage = user.UserImageName,
                Job = user.Job,
                City = user.City,
                Role = user.Role
            };

            await context.RespondAsync(response);
        }
    }

}
