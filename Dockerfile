FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY AU.Domain/AU.Domain.csproj AU.Domain/
COPY AU.Application/AU.Application.csproj AU.Application/
COPY AU.Infrastructure/AU.Infrastructure.csproj AU.Infrastructure/
COPY AU.Api/AU.Api.csproj AU.Api/
COPY AU.CheckScheduler/AU.CheckScheduler.csproj AU.CheckScheduler/
COPY AU.CheckWorker/AU.CheckWorker.csproj AU.CheckWorker/
COPY AU.Jobs/AU.Jobs.csproj AU.Jobs/
COPY AU.NotificationWorker/AU.NotificationWorker.csproj AU.NotificationWorker/
RUN for p in AU.Api AU.CheckScheduler AU.CheckWorker AU.Jobs AU.NotificationWorker; do \
      dotnet restore "$p/$p.csproj" || exit 1; \
    done

FROM restore AS build
COPY . .
RUN for p in AU.Api AU.CheckScheduler AU.CheckWorker AU.Jobs AU.NotificationWorker; do \
      dotnet publish "$p/$p.csproj" -c Release -o "/app/publish/$p" --no-restore /p:UseAppHost=false || exit 1; \
    done

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG PROJECT
ENV APP_DLL=$PROJECT.dll
WORKDIR /app
COPY --from=build /app/publish/$PROJECT .
RUN mkdir -p /app/storage && chown -R $APP_UID /app/storage
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]
