FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY . .
RUN dotnet publish -c Release -o /app

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS final
WORKDIR /app
COPY --from=build /app .

ENV RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "ClientService.dll"]
