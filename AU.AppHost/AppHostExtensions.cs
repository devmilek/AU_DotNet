namespace CCS.AppHost;

static class AppHostExtensions
{
    public static IResourceBuilder<T> WithBackingServices<T>(
        this IResourceBuilder<T> project,
        IResourceBuilder<IResourceWithConnectionString> db,
        IResourceBuilder<IResourceWithConnectionString> rabbit)
        where T : IResourceWithEnvironment, IResourceWithWaitSupport
        => project
            .WithReference(db).WithReference(rabbit)
            .WaitFor(db).WaitFor(rabbit);

    public static IResourceBuilder<T> WithMailpit<T>(
        this IResourceBuilder<T> project,
        IResourceBuilder<ContainerResource> mailpit)
        where T : IResourceWithEnvironment, IResourceWithWaitSupport
    {
        var smtp = mailpit.GetEndpoint("smtp");

        return project
            .WithEnvironment("Smtp__Host", smtp.Property(EndpointProperty.Host))
            .WithEnvironment("Smtp__Port", smtp.Property(EndpointProperty.Port))
            .WithEnvironment("Smtp__Username", "mailpit")
            .WithEnvironment("Smtp__Password", "mailpit")
            .WithEnvironment("Smtp__FromAddress", "uptime@localhost")
            .WaitFor(mailpit);
    }
}