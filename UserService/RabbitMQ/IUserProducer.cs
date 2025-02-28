using SharedLibrary;
namespace UserService.RabbitMQ
{
    public interface IUserProducer
    {
        Task PublishUser(PublishedUser userPublished);
        //Task SendUserAsync(PublishedUser user);
    }
}
