namespace AU.Application.Channels.Queries.GetNotificationChannels;

public sealed record GetNotificationChannelsQuery(Guid OrganizationId);

public sealed class GetNotificationChannelsHandler
{
    public Task<IReadOnlyList<ChannelListRow>> Handle(
        GetNotificationChannelsQuery query,
        INotificationChannelRepository channelRepository,
        CancellationToken cancellationToken) =>
        channelRepository.ListAsync(query.OrganizationId, cancellationToken);
}
