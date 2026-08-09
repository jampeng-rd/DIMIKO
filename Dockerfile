# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080


# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# 先複製 csproj，讓 Docker 可以有效利用 restore cache
COPY ["ECommerce.Web/ECommerce.Web.csproj", "ECommerce.Web/"]
COPY ["ECommerce.Business/ECommerce.Business.csproj", "ECommerce.Business/"]
COPY ["ECommerce.DataAccess/ECommerce.DataAccess.csproj", "ECommerce.DataAccess/"]
COPY ["ECommerce.Models/ECommerce.Models.csproj", "ECommerce.Models/"]
COPY ["ECommerce.Utility/ECommerce.Utility.csproj", "ECommerce.Utility/"]

RUN dotnet restore "ECommerce.Web/ECommerce.Web.csproj"

# 再複製全部原始碼
COPY . .

WORKDIR "/src/ECommerce.Web"

RUN dotnet publish "ECommerce.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Final stage
# =========================
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ECommerce.Web.dll"]
