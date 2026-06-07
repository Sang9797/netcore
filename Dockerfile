FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CqrsOrderService.slnx ./
COPY src/Cqrs.OrderService/Cqrs.OrderService.csproj src/Cqrs.OrderService/
RUN dotnet restore src/Cqrs.OrderService/Cqrs.OrderService.csproj
COPY src/Cqrs.OrderService src/Cqrs.OrderService
RUN dotnet publish src/Cqrs.OrderService/Cqrs.OrderService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cqrs.OrderService.dll"]
