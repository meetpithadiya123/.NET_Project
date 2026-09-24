# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["E-Commerce-Website/E-Commerce-Website.csproj", "E-Commerce-Website/"]
RUN dotnet restore "E-Commerce-Website/E-Commerce-Website.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/E-Commerce-Website"
RUN dotnet publish "E-Commerce-Website.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Port binding for Render/cloud hosts
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E-Commerce-Website.dll"]