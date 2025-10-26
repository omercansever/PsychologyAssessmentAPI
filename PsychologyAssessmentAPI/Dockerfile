# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5128

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PsychologyAssessmentAPI.csproj", "./"]
RUN dotnet restore "./PsychologyAssessmentAPI.csproj"
COPY . .
RUN dotnet publish "PsychologyAssessmentAPI.csproj" -c Release -o /app/publish

FROM base AS finalf
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PsychologyAssessmentAPI.dll"]
