FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 4000
ENV ASPNETCORE_URLS=http://+:4000
ENV StemmingServiceUrl http://localhost:5000
ENV CoordinatorServiceUrl http://localhost:3000
ENV MongoDbConnectionString=mongodb://localhost:27017

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["WorkerService.csproj", "./"]
RUN dotnet restore "WorkerService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Properties", "./Properties"]
CMD str=$ASPNETCORE_URLS \
	&& readlink -f $(which sh) \
	&& newMyUrl=${str#*:*:} \
	&& newLine2="\"MyUrl\": \"http:\/\/worker1:$newMyUrl\"," \
	&& sed -i "2s/.*/$newLine2/" ./Properties/configuration.json \
	&& newLine3=$(echo "\"CoordinatorServiceUrl\": \"$CoordinatorServiceUrl\"," | sed 's/\//\\\//g') \
	&& sed -i "3s/.*/$newLine3/" ./Properties/configuration.json \
	&& newLine4=$(echo "\"StemmingServiceUrl\": \"$StemmingServiceUrl\"," | sed 's/\//\\\//g') \
	&& sed -i "4s/.*/$newLine4/" ./Properties/configuration.json \
	&& newLine5=$(echo "\"MongoDbConnectionString\": \"$MongoDbConnectionString\"," | sed 's/\//\\\//g') \
	&& sed -i "5s/.*/$newLine5/" ./Properties/configuration.json \
	&& cat ./Properties/configuration.json \
	&& dotnet WorkerService.dll
