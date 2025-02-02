FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /App

COPY ProductManagement.Presentation/ProductManagement.Presentation.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish ./ProductManagement.Presentation/ProductManagement.Presentation.csproj -c Release -o out

RUN dotnet tool install --global dotnet-ef --version 9.0
ENV PATH="/root/.dotnet/tools:${PATH}"
