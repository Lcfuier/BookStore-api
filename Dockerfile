# ----------------------------------
# Build stage
# ----------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Chỉ copy những gì cần để restore trước → tăng tốc
COPY ["BookStore.BLL.sln", "./"]
COPY ["BookStore.Api/BookStore.Api.csproj", "BookStore.Api/"]
COPY ["BookStore.Application/BookStore.Application.csproj", "BookStore.Application/"]
COPY ["BookStore.Domain/BookStore.Domain.csproj", "BookStore.Domain/"]
COPY ["BookStore.Infrastructure/BookStore.Infrastructure.csproj", "BookStore.Infrastructure/"]

# Nếu không chạy unit test trong container, không cần dòng này:
COPY ["BookStore.UnitTest/BookStore.UnitTest.csproj", "BookStore.UnitTest/"]

# Khôi phục package
RUN dotnet restore "BookStore.BLL.sln"

# Copy toàn bộ mã nguồn (sau restore mới copy full để tránh cache sai)
COPY . .

# Build dự án
WORKDIR "/src/BookStore.Api"
RUN dotnet build "BookStore.Api.csproj" -c Release -o /app/build

# ----------------------------------
# Publish stage
# ----------------------------------
FROM build AS publish
RUN dotnet publish "BookStore.Api.csproj" -c Release -o /app/publish --no-restore

# ----------------------------------
# Runtime stage
# ----------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Mặc định ASP.NET Core chạy ở port 80
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "BookStore.Api.dll"]
