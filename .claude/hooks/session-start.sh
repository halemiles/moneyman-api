#!/bin/bash
# SessionStart hook: provision the .NET SDK so Claude Code on the web can
# build, run the analyzers (Sonar/CA findings — see issue #157) and run tests.
#
# Notes:
# - .NET 9 (this repo's target framework) reached end-of-support and has been
#   pulled from the package feeds, and the SDK tarball host
#   (builds.dotnet.microsoft.com) is outside the network egress allowlist.
#   The .NET 10 SDK (in the Ubuntu 24.04 archive) builds and tests net9.0
#   projects fine, with DOTNET_ROLL_FORWARD=Major letting net9.0 test
#   assemblies run on the 10.0 runtime.
# - Runs only in the remote (web) environment; local CLI sessions use the
#   developer's own SDK.
set -euo pipefail

# Only provision in Claude Code on the web; local sessions are left untouched.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Persist .NET environment for the whole session.
if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo 'export DOTNET_CLI_TELEMETRY_OPTOUT=1'
    echo 'export DOTNET_NOLOGO=1'
    # net9.0 build/test artifacts roll forward onto the installed 10.0 runtime.
    echo 'export DOTNET_ROLL_FORWARD=Major'
  } >> "$CLAUDE_ENV_FILE"
fi

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_ROLL_FORWARD=Major

# Install the .NET 10 SDK if it isn't already present (idempotent).
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
  # Tolerate failures from third-party PPAs that are blocked by the egress
  # allowlist; the Ubuntu archive lists we need still refresh successfully.
  apt-get update || true
  DEBIAN_FRONTEND=noninteractive apt-get install -y dotnet-sdk-10.0
fi

dotnet --version

# Warm the NuGet restore so the first build in-session is fast. The container
# state is cached after the hook completes, so this persists across the session.
dotnet restore "$CLAUDE_PROJECT_DIR/src/Moneyman.Api.sln"
