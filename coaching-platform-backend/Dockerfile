# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["JokerNutrition.Api/JokerNutrition.Api.csproj", "JokerNutrition.Api/"]
COPY ["JokerNutrition.Business/JokerNutrition.Business.csproj", "JokerNutrition.Business/"]
COPY ["JokerNutrition.Data/JokerNutrition.Data.csproj", "JokerNutrition.Data/"]
RUN dotnet restore "JokerNutrition.Api/JokerNutrition.Api.csproj"

# Copy backend files and build
COPY . .
WORKDIR "/src/JokerNutrition.Api"
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
