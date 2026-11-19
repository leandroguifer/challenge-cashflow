FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Cashflow.Core/Cashflow.Core.csproj src/Cashflow.Core/
COPY src/Cashflow.Balance/Cashflow.Balance.csproj src/Cashflow.Balance/
RUN dotnet restore src/Cashflow.Balance/Cashflow.Balance.csproj

COPY src/Cashflow.Core/ src/Cashflow.Core/
COPY src/Cashflow.Balance/ src/Cashflow.Balance/
RUN dotnet publish src/Cashflow.Balance/Cashflow.Balance.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Cashflow.Balance.dll"]