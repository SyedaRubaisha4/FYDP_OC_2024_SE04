using MassTransit;
using SharedLibrary;

namespace FeedbackService.RabbitMQ
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
                var response = await _requestClient.GetResponse<UserResponseMessage>(new UserRequestMessage { UserId = userId });
                return response.Message.User;
            }
        }
}
