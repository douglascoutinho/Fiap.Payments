FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8084

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos de projeto respeitando a estrutura de pastas da solução
COPY ["Fiap.Payments.Api/Fiap.Payments.Api.csproj", "Fiap.Payments.Api/"]
COPY ["Fiap.Payments.Domain/Fiap.Payments.Domain.csproj", "Fiap.Payments.Domain/"]

# Restaura as dependências a partir do projeto principal
RUN dotnet restore "Fiap.Payments.Api/Fiap.Payments.Api.csproj"

# Copia todo o código fonte
COPY . .

# Define o diretório de trabalho no projeto principal e compila
WORKDIR "/src/Fiap.Payments.Api"
RUN dotnet build "Fiap.Payments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fiap.Payments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_URLS="http://+:8084"
ENV ASPNETCORE_ENVIRONMENT="Development"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fiap.Payments.Api.dll"]
