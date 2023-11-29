#!/bin/bash

dotnet test --collect:"XPlat Code Coverage"
dotnet ~/.nuget/packages/reportgenerator/5.1.10/tools/net7.0/ReportGenerator.dll "-reports:*/**/coverage.cobertura.xml" "-targetdir:TestResults" -reporttypes:Html "-historydir:history"
