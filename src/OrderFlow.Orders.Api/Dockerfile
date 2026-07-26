# Etapa 1: build — usa el SDK completo (pesado, pero solo existe en esta etapa)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos solo los .csproj primero, para aprovechar el cache de capas de Docker:
# si el código cambia pero las dependencias no, "dotnet restore" no se repite.
COPY ["src/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/OrderFlow.Contracts/"]
COPY ["src/OrderFlow.Orders.Api/OrderFlow.Orders.Api.csproj", "src/OrderFlow.Orders.Api/"]
RUN dotnet restore "src/OrderFlow.Orders.Api/OrderFlow.Orders.Api.csproj"

# Ahora sí copiamos el código fuente completo y publicamos
COPY src/OrderFlow.Contracts/ src/OrderFlow.Contracts/
COPY src/OrderFlow.Orders.Api/ src/OrderFlow.Orders.Api/
WORKDIR /src/src/OrderFlow.Orders.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa 2: runtime — imagen liviana, solo lo necesario para EJECUTAR (no compilar)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderFlow.Orders.Api.dll"]