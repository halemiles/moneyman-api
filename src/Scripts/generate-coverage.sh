#!/bin/bash

dotnet test --collect:"XPlat Code Coverage"
reportgenerator "-reports:*/**/coverage.cobertura.xml" "-targetdir:TestResults" -reporttypes:Html;HtmlInline "-historydir:history"
