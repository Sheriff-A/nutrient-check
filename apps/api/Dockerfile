# Dev-oriented image: builds and runs the API from source. Not production-hardened
# (no multi-stage slim runtime image, no non-root user) - that's Phase 4 scope.
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src
COPY . .

RUN dotnet restore NutriCheck.sln

EXPOSE 8080

CMD ["dotnet", "run", "--project", "src/NutriCheck.Api/NutriCheck.Api.csproj", "--no-launch-profile"]
