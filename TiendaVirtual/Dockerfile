FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TiendaVirtual/TiendaVirtual.csproj", "TiendaVirtual/"]
RUN dotnet restore "TiendaVirtual/TiendaVirtual.csproj"
COPY . .
WORKDIR "/src/TiendaVirtual"
RUN dotnet build "TiendaVirtual.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TiendaVirtual.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "TiendaVirtual.dll"]
