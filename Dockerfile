ARG CI_REGISTRY_IMAGE

FROM $CI_REGISTRY_IMAGE:builder AS deps

RUN mkdir -p /src/packages

FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

ENV NUGET_PACKAGES=/src/packages

COPY --from=deps $NUGET_PACKAGES/. $NUGET_PACKAGES/

WORKDIR /src

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

COPY ["src/Fanex.DepositPlayerAPI.Domain/Fanex.DepositPlayerAPI.Domain.csproj", "./Fanex.DepositPlayerAPI.Domain/"]
RUN dotnet restore "Fanex.DepositPlayerAPI.Domain/Fanex.DepositPlayerAPI.Domain.csproj" -v n -r linux-x64

COPY ["src/Fanex.DepositPlayerAPI.Application/Fanex.DepositPlayerAPI.Application.csproj", "./Fanex.DepositPlayerAPI.Application/"]
RUN dotnet restore "Fanex.DepositPlayerAPI.Application/Fanex.DepositPlayerAPI.Application.csproj" -v n -r linux-x64

COPY ["src/Fanex.DepositPlayerAPI.Infrastructure/Fanex.DepositPlayerAPI.Infrastructure.csproj", "./Fanex.DepositPlayerAPI.Infrastructure/"]
RUN dotnet restore "Fanex.DepositPlayerAPI.Infrastructure/Fanex.DepositPlayerAPI.Infrastructure.csproj" -v n -r linux-x64

COPY ["src/Fanex.DepositPlayerAPI.Presentation/Fanex.DepositPlayerAPI.Presentation.csproj", "./Fanex.DepositPlayerAPI.Presentation/"]
RUN dotnet restore "Fanex.DepositPlayerAPI.Presentation/Fanex.DepositPlayerAPI.Presentation.csproj" -v n -r linux-x64

COPY ["src/Fanex.DepositPlayerAPI.WebApi/Fanex.DepositPlayerAPI.WebApi.csproj", "./Fanex.DepositPlayerAPI.WebApi/"]
RUN dotnet restore "Fanex.DepositPlayerAPI.WebApi/Fanex.DepositPlayerAPI.WebApi.csproj" -v n -r linux-x64

COPY src .

RUN dotnet publish "Fanex.DepositPlayerAPI.WebApi/Fanex.DepositPlayerAPI.WebApi.csproj" -c Release -o /publish -r linux-x64 \
    --use-current-runtime \
    --self-contained false

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine-25h1 AS final

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000

# Dont run as root
RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

WORKDIR /app

COPY --from=builder /publish .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Fanex.DepositPlayerAPI.WebApi.dll"]
