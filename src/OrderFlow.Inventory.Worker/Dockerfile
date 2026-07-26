FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OrderFlow.Contracts/OrderFlow.Contracts.csproj", "src/OrderFlow.Contracts/"]
COPY ["src/OrderFlow.Inventory.Worker/OrderFlow.Inventory.Worker.csproj", "src/OrderFlow.Inventory.Worker/"]
RUN dotnet restore "src/OrderFlow.Inventory.Worker/OrderFlow.Inventory.Worker.csproj"

COPY src/OrderFlow.Contracts/ src/OrderFlow.Contracts/
COPY src/OrderFlow.Inventory.Worker/ src/OrderFlow.Inventory.Worker/
WORKDIR /src/src/OrderFlow.Inventory.Worker
RUN dotnet publish -c Release -o /app/publish --no-restore

# Nota: usamos "runtime" (no "aspnet") porque este servicio no expone HTTP —
# es un Worker Service puro, no necesita el pipeline web de ASP.NET Core.
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Inventory.Worker.dll"]