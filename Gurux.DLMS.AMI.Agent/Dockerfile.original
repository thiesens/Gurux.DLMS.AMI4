FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Gurux.DLMS.AMI.Agent/Gurux.DLMS.AMI.Agent.csproj", "Gurux.DLMS.AMI.Agent/"]
RUN dotnet restore "Gurux.DLMS.AMI.Agent/Gurux.DLMS.AMI.Agent.csproj"
COPY . .
WORKDIR "/src/Gurux.DLMS.AMI.Agent"
RUN dotnet build "Gurux.DLMS.AMI.Agent.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Gurux.DLMS.AMI.Agent.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gurux.DLMS.AMI.Agent.dll"]
