FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 61759
EXPOSE 44352

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["MsExperience.DevAzure.Front/MsExperience.DevAzure.Front.csproj", "MsExperience.DevAzure.Front/"]
RUN dotnet restore "MsExperience.DevAzure.Front/MsExperience.DevAzure.Front.csproj"
COPY . .
WORKDIR "/src/MsExperience.DevAzure.Front"
RUN dotnet build "MsExperience.DevAzure.Front.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MsExperience.DevAzure.Front.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MsExperience.DevAzure.Front.dll"]