FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PsychologyAssessmentAPI/PsychologyAssessmentAPI.csproj", "PsychologyAssessmentAPI/"]
RUN dotnet restore "PsychologyAssessmentAPI/PsychologyAssessmentAPI.csproj"
COPY . .
WORKDIR "/src/PsychologyAssessmentAPI"
RUN dotnet build "PsychologyAssessmentAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PsychologyAssessmentAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PsychologyAssessmentAPI.dll"]
