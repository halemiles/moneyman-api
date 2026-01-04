#!/bin/bash

set -e  # Exit on error

# ANSI color codes
GREEN="\033[1;32m"
YELLOW="\033[1;33m"
NC="\033[0m"  # No color

echo "${YELLOW}Installing .NET SDK... ${NC}"
wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel latest --channel STS > /dev/null 2>&1
#rm -f dotnet-install.sh  # Clean up the install script
echo  "${GREEN}✔ Completed.${NC}"

echo  "${YELLOW}Installing Python pipx for pre-commit... ${NC}"
sudo apt-get install -y pipx > /dev/null 2>&1
pipx install pre-commit > /dev/null 2>&1
echo  "${GREEN}✔ Completed.${NC}"

echo  "${YELLOW}Installing Node.js and npm... ${NC}"
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - > /dev/null 2>&1
sudo apt-get install -y nodejs > /dev/null 2>&1
echo  "${GREEN}✔ Completed.${NC}"

echo "${GREEN}✔ Installation completed. Please restart your terminal.${NC}"
