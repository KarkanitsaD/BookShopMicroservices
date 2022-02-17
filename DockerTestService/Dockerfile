#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#create image base on ...
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
#now we in app folder
WORKDIR /app
EXPOSE 80

#all that we need to build application
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#we in src
WORKDIR /src
#copy csproj to current location
COPY ["DockerTestService.csproj", "./"]
#restore
RUN dotnet restore "DockerTestService.csproj"
#copy all other files that we needed
COPY . .
RUN dotnet publish "DockerTestService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DockerTestService.dll"]