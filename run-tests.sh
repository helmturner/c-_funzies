#!/bin/bash

# Colors for better output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Function to run tests with proper output
run_tests() {
    local test_project=$1
    local test_type=$2
    
    echo -e "${YELLOW}Running $test_type tests...${NC}"
    if dotnet test "$test_project" --no-restore; then
        echo -e "${GREEN}$test_type tests passed!${NC}"
        return 0
    else
        echo -e "${RED}$test_type tests failed!${NC}"
        return 1
    fi
}

# Restore packages
echo -e "${YELLOW}Restoring packages...${NC}"
dotnet restore
echo

# Run each test type based on arguments
if [ $# -eq 0 ] || [ "$1" == "all" ]; then
    # Run all test types
    run_tests "funzies.Tests.Unit/funzies.Tests.Unit.csproj" "Unit"
    unit_result=$?
    echo
    
    run_tests "funzies.Tests.Integration/funzies.Tests.Integration.csproj" "Integration"
    integration_result=$?
    echo
    
    run_tests "funzies.Tests.E2E/funzies.Tests.E2E.csproj" "End-to-End"
    e2e_result=$?
    
    # Report overall status
    echo
    if [ $unit_result -eq 0 ] && [ $integration_result -eq 0 ] && [ $e2e_result -eq 0 ]; then
        echo -e "${GREEN}All tests passed successfully!${NC}"
        exit 0
    else
        echo -e "${RED}Some tests failed. Check the output above for details.${NC}"
        exit 1
    fi
elif [ "$1" == "unit" ]; then
    run_tests "funzies.Tests.Unit/funzies.Tests.Unit.csproj" "Unit"
    exit $?
elif [ "$1" == "integration" ]; then
    run_tests "funzies.Tests.Integration/funzies.Tests.Integration.csproj" "Integration"
    exit $?
elif [ "$1" == "e2e" ]; then
    run_tests "funzies.Tests.E2E/funzies.Tests.E2E.csproj" "End-to-End"
    exit $?
else
    echo -e "${RED}Invalid test type: $1${NC}"
    echo "Usage: $0 [all|unit|integration|e2e]"
    exit 1
fi
