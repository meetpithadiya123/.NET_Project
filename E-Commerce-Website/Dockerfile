# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["E-Commerce-Website.csproj", "./"]
RUN dotnet restore "E-Commerce-Website.csproj"

# Copy the rest of the files and publish
COPY . .
RUN dotnet publish "E-Commerce-Website.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Port binding for Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E-Commerce-Website.dll"]﻿
