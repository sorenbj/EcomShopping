FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EcomShopping.API/EcomShopping.API.csproj", "src/EcomShopping.API/"]
COPY ["src/EcomShopping.Infrastructure/EcomShopping.Infrastructure.csproj", "src/EcomShopping.Infrastructure/"]
COPY ["src/EcomShopping.Application/EcomShopping.Application.csproj", "src/EcomShopping.Application/"]
COPY ["src/EcomShopping.Domain/EcomShopping.Domain.csproj", "src/EcomShopping.Domain/"]
RUN dotnet restore "src/EcomShopping.API/EcomShopping.API.csproj"
COPY . .
WORKDIR "/src/src/EcomShopping.API"
RUN dotnet build "EcomShopping.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcomShopping.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomShopping.API.dll"]
