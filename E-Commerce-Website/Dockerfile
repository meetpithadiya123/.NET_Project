# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file using the repository root path
COPY ["E-Commerce-Website/E-Commerce-Website.csproj", "E-Commerce-Website/"]
RUN dotnet restore "E-Commerce-Website/E-Commerce-Website.csproj"

# Copy all source files and publish release binaries
COPY . .
WORKDIR "/src/E-Commerce-Website"
RUN dotnet publish "E-Commerce-Website.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configure container port for Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E-Commerce-Website.dll"]
