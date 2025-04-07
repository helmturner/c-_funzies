#!/bin/bash

# Script to run all tests in the solution

echo "=== Running Unit Tests ==="
dotnet test funzies.Tests.Unit/funzies.Tests.Unit.csproj

echo ""
echo "=== Running Integration Tests ==="
dotnet test funzies.Tests.Integration/funzies.Tests.Integration.csproj

echo ""
echo "=== Running E2E Tests ==="
dotnet test funzies.Tests.E2E/funzies.Tests.E2E.csproj

echo ""
echo "=== All Tests Completed ==="
