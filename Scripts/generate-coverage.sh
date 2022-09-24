#!/bin/bash

cd ../Moneyman.Tests
dotnet test --collect:"XPlat Cod Coverage"
dotnet ~/.nuget/packages/reportgenerator/5.1.10/tools/net6.0/ReportGenerator.dll "-reports:*/**/coverage.cobertura.xml" "-targetdir:TestResults" -reporttypes:Html "-historydir:history"e
