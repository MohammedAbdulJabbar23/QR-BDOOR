FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AlBadour.sln ./
COPY AlBadour.Domain/AlBadour.Domain.csproj AlBadour.Domain/
COPY AlBadour.Application/AlBadour.Application.csproj AlBadour.Application/
COPY AlBadour.Infrastructure/AlBadour.Infrastructure.csproj AlBadour.Infrastructure/
COPY AlBadour.WebApi/AlBadour.WebApi.csproj AlBadour.WebApi/
RUN dotnet restore AlBadour.WebApi/AlBadour.WebApi.csproj

COPY AlBadour.Domain/ AlBadour.Domain/
COPY AlBadour.Application/ AlBadour.Application/
COPY AlBadour.Infrastructure/ AlBadour.Infrastructure/
COPY AlBadour.WebApi/ AlBadour.WebApi/
RUN dotnet publish AlBadour.WebApi/AlBadour.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "AlBadour.WebApi.dll"]
