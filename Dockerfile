FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Aliyar.Web/Aliyar.Web.csproj Aliyar.Web/
RUN dotnet restore Aliyar.Web/Aliyar.Web.csproj

COPY Aliyar.Web/ Aliyar.Web/
RUN dotnet publish Aliyar.Web/Aliyar.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]
