#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Chapter3.Examples.LOH.Web/Chapter3.Examples.LOH.Web.csproj", "Chapter3.Examples.LOH.Web/"]
RUN dotnet restore "Chapter3.Examples.LOH.Web/Chapter3.Examples.LOH.Web.csproj"
COPY . .
WORKDIR "/src/Chapter3.Examples.LOH.Web"
RUN dotnet build "Chapter3.Examples.LOH.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chapter3.Examples.LOH.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chapter3.Examples.LOH.Web.dll"]