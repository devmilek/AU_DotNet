using Microsoft.Extensions.FileProviders;
using AU.Application.Abstractions;
using AU.Infrastructure.Storage;

namespace AU.Api.Storage;

public static class FileStorageSetup
{
    public static WebApplication UseLocalFileStorage(this WebApplication app)
    {
        if (app.Services.GetRequiredService<IFileStorage>() is not LocalFileStorage storage)
            return app;

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = LocalFileStorage.RequestPath,
            OnPrepareResponse = context =>
            {
                var headers = context.Context.Response.Headers;
                headers.CacheControl = "public, max-age=31536000, immutable";
                headers.XContentTypeOptions = "nosniff";
            }
        });

        return app;
    }
}
