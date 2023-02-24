FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ComplicatedInsertController/ComplicatedInsertController.csproj", "ComplicatedInsertController/"]
RUN dotnet restore "ComplicatedInsertController/ComplicatedInsertController.csproj"
COPY . .
WORKDIR "/src/ComplicatedInsertController"
RUN dotnet build "ComplicatedInsertController.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComplicatedInsertController.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComplicatedInsertController.dll"]
