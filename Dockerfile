# ----------------------------------
# Build stage
# ----------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY ["BookStore.BLL.sln", "./"]
COPY ["BookStore.Api/BookStore.Api.csproj", "BookStore.Api/"]
COPY ["BookStore.Application/BookStore.Application.csproj", "BookStore.Application/"]
COPY ["BookStore.Domain/BookStore.Domain.csproj", "BookStore.Domain/"]
COPY ["BookStore.Infrastructure/BookStore.Infrastructure.csproj", "BookStore.Infrastructure/"]
COPY ["BookStore.UnitTest/BookStore.UnitTest.csproj", "BookStore.UnitTest/"]  

# Restore dependencies
RUN dotnet restore "BookStore.BLL.sln"

# Copy rest of the code (bao gồm tất cả thư mục dự án)
COPY . .

# Build
WORKDIR "/src/BookStore.Api"
RUN dotnet build "BookStore.Api.csproj" -c Release -o /app/build

# ----------------------------------
# Publish stage
# ----------------------------------
FROM build AS publish
RUN dotnet publish "BookStore.Api.csproj" -c Release -o /app/publish

# ----------------------------------
# Runtime stage
# ----------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose default ASP.NET port
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "BookStore.Api.dll"]
