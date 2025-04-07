#!/bin/bash

# Set the environment to test
export ASPNETCORE_ENVIRONMENT=Test

# Change to the E2E test directory
cd "$(dirname "$0")/funzies.Tests/E2E"

# Install Playwright browsers if needed
dotnet add package Microsoft.Playwright.CLI --version 1.51.0
dotnet build
dotnet tool install --global Microsoft.Playwright.CLI --version 1.51.0 || true
playwright install

# Run the E2E tests
dotnet test