FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EcomShopping.Web/EcomShopping.Web.csproj", "src/EcomShopping.Web/"]
RUN dotnet restore "src/EcomShopping.Web/EcomShopping.Web.csproj"
COPY . .
WORKDIR "/src/src/EcomShopping.Web"
RUN dotnet build "EcomShopping.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcomShopping.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomShopping.Web.dll"]
