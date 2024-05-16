FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5136;https://+:7136
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="123456"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/fakebridge.pfx
COPY ["fakebridge.pfx", "/https/fakebridge.pfx"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fake.Detection.Post.Bridge.Api/Fake.Detection.Post.Bridge.Api.csproj", "src/Fake.Detection.Post.Bridge.Api/"]
COPY ["src/Fake.Detection.Post.Bridge.Dal/Fake.Detection.Post.Bridge.Dal.csproj", "src/Fake.Detection.Post.Bridge.Dal/"]
COPY ["src/Fake.Detection.Post.Bridge.Bll/Fake.Detection.Post.Bridge.Bll.csproj", "src/Fake.Detection.Post.Bridge.Bll/"]
RUN dotnet nuget add source "https://nuget.pkg.github.com/Fake-Detector/index.json" -n github -u <USER> -p <PASSWORD> --store-password-in-clear-text
RUN dotnet restore "src/Fake.Detection.Post.Bridge.Api/Fake.Detection.Post.Bridge.Api.csproj"
COPY . .
WORKDIR "/src/src/Fake.Detection.Post.Bridge.Api"
RUN dotnet build "Fake.Detection.Post.Bridge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fake.Detection.Post.Bridge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fake.Detection.Post.Bridge.Api.dll"]
