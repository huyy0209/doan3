FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY DoAnWeb/DoAnWeb.csproj DoAnWeb/
RUN dotnet restore DoAnWeb/DoAnWeb.csproj
COPY DoAnWeb/ DoAnWeb/
RUN dotnet publish DoAnWeb/DoAnWeb.csproj -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/logs /backups \
    && chown -R 10001:0 /app /backups
WORKDIR /app
COPY --from=build --chown=10001:0 /out/ .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD curl --fail --silent http://127.0.0.1:8080/suc-khoe || exit 1
USER 10001
ENTRYPOINT ["dotnet", "DoAnWeb.dll"]
