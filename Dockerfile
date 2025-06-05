# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution và project files vào container
COPY BookStore.sln .
COPY BookStore.API/BookStore.Api.csproj BookStore.Api/
COPY BookStore.Application/BookStore.Application.csproj BookStore.Application/
COPY BookStore.Domain/BookStore.Domain.csproj BookStore.Domain/
COPY BookStore.Infrastructure/BookStore.Infrastructure.csproj BookStore.Infrastructure/

# Restore các package nuget
RUN dotnet restore

# Copy toàn bộ source code
COPY . .

# Build và publish project API
RUN dotnet publish BookStore.API/BookStore.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy thư mục publish từ stage build
COPY --from=build /app/publish .

# Mở port 8080 cho Render
EXPOSE 8080

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "BookStore.API.dll"]
