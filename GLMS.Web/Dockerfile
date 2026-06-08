FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["GLMS/GLMS.csproj", "GLMS/"]
RUN dotnet restore "GLMS/GLMS.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GLMS"
RUN dotnet build "GLMS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GLMS.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GLMS.dll"]