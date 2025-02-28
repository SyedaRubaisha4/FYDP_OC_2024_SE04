using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using MassTransit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace UserService.RabbitMQ
{
    public class UserProducer : IUserProducer
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserProducer> _logger;



        public UserProducer(IPublishEndpoint publishEndpoint, ILogger<UserProducer> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishUser(PublishedUser userPublished)
        {
            _logger.LogInformation("Publishing user: {Name}, {PhoneNumber}", userPublished.Name, userPublished.PhoneNumber);
            await _publishEndpoint.Publish<PublishedUser>(userPublished);

        }
    }
}
