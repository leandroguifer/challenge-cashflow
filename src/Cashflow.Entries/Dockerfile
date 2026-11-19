FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Cashflow.Core/Cashflow.Core.csproj src/Cashflow.Core/
COPY src/Cashflow.Entries/Cashflow.Entries.csproj src/Cashflow.Entries/
RUN dotnet restore src/Cashflow.Entries/Cashflow.Entries.csproj
COPY src/Cashflow.Core src/Cashflow.Core
COPY src/Cashflow.Entries src/Cashflow.Entries
RUN dotnet publish src/Cashflow.Entries/Cashflow.Entries.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cashflow.Entries.dll"]