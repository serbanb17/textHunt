FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 3000
ENV ASPNETCORE_URLS=http://+:3000
ENV DbInitServiceUrl=http://localhost:6000
ENV StemmingServiceUrl=http://localhost:5000
ENV MongoDbConnectionString=mongodb://localhost:27017

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["CoordinatorService.csproj", "./"]
RUN dotnet restore "CoordinatorService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CoordinatorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoordinatorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Properties", "./Properties"]
CMD newLine2=$(echo "\"DbInitServiceUrl\": \"$DbInitServiceUrl\"," | sed 's/\//\\\//g') \
	&& sed -i "2s/.*/$newLine2/" ./Properties/configuration.json \
	&& newLine3=$(echo "\"StemmingServiceUrl\": \"$StemmingServiceUrl\"," | sed 's/\//\\\//g') \
	&& sed -i "3s/.*/$newLine3/" ./Properties/configuration.json \
	&& newLine4=$(echo "\"MongoDbConnectionString\": \"$MongoDbConnectionString\"," | sed 's/\//\\\//g') \
	&& sed -i "4s/.*/$newLine4/" ./Properties/configuration.json \
	&& cat ./Properties/configuration.json \
	&& dotnet CoordinatorService.dll
