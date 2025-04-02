
using MassTransit;
using SharedLibrary;
namespace Location_Service.RabbitMQ
{

    public class UserRequestProducer
    {

        private readonly IRequestClient<UserRequestMessage> _requestClient;

        public UserRequestProducer(IRequestClient<UserRequestMessage> requestClient)
        {
            _requestClient = requestClient;
        }

        public async Task<PublishedUser> RequestUserById(string userId)
        {
            try
            {
                var response = await _requestClient
                    .GetResponse<UserResponseMessage>(
                        new UserRequestMessage { UserId = userId },
                        timeout: TimeSpan.FromSeconds(60) // Increase timeout
                    );

                return response.Message.User;
            }
            catch (RequestTimeoutException)
            {
                throw new Exception("User service did not respond in time.");
            }
        }
    }
    }


