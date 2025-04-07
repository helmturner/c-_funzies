#!/bin/bash

# Set the environment to test
export ASPNETCORE_ENVIRONMENT=Test

# Change to the E2E test directory
cd "$(dirname "$0")/funzies.Tests/E2E"

# Build the project first
dotnet build

# Install the Playwright tool
dotnet tool install --global Microsoft.Playwright.CLI || true

# Try to install browsers with fallbacks for different platforms
if command -v playwright &>/dev/null; then
  playwright install
elif command -v pwsh &>/dev/null; then
  pwsh -Command "playwright install"
else
  echo "Installing browsers via dotnet CLI"
  dotnet exec ~/.dotnet/tools/playwright.dll install
fi

# Run the E2E tests
dotnet test
