# Giai đoạn build (sử dụng SDK để compile)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy file .csproj và restore NuGet packages
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ source code và publish
COPY . ./
RUN dotnet publish -c Release -o out

# Giai đoạn runtime (sử dụng image nhẹ hơn)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Cấu hình môi trường
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000  # Port mặc định của Render
EXPOSE 10000

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "OHCP_BK.dll"]  # Thay YourProjectName bằng tên project từ .csproj