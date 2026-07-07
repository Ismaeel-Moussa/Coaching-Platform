# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["coaching-platform-backend/JokerNutrition.Api/JokerNutrition.Api.csproj", "coaching-platform-backend/JokerNutrition.Api/"]
COPY ["coaching-platform-backend/JokerNutrition.Business/JokerNutrition.Business.csproj", "coaching-platform-backend/JokerNutrition.Business/"]
COPY ["coaching-platform-backend/JokerNutrition.Data/JokerNutrition.Data.csproj", "coaching-platform-backend/JokerNutrition.Data/"]
RUN dotnet restore "coaching-platform-backend/JokerNutrition.Api/JokerNutrition.Api.csproj"

# Copy backend files and build
COPY coaching-platform-backend/ coaching-platform-backend/
WORKDIR "/src/coaching-platform-backend/JokerNutrition.Api"
RUN dotnet build "JokerNutrition.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "JokerNutrition.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 10000 (Render default) and run the application
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "JokerNutrition.Api.dll"]
