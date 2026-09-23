using System.Reflection.Metadata;
using US.Domain.Entities;
using US.Domain.Enums;
using US.Domain.ValueObjects;

namespace US.Application.Channels.Commands.CreateNotificationChannel;

public class CreateNotificationChannelHandler
{
    public async Task<Guid> Handle(CreateNotificationChannelCommand command, INotificationChannelRepository channelRepository, IUnitOfWork unitOfWork)
    {
        var channel = command.Type switch
        {
            ChannelType.Email => NotificationChannel.CreateEmail(
                command.OrganizationId,
                command.Name,
                EmailChannelConfig.Create(command.EmailTo!)),

            _ => throw new NotSupportedException($"Typ kanału {command.Type} nie jest jeszcze wspierany.")
        };
        
        channelRepository.Add(channel);
        await unitOfWork.SaveChangesAsync();
        
        return channel.Id;
    }
}