#!/usr/bin/env bash
# PreToolUse guard for Bash: blocks bare env/printenv/set dumps and any
# command that references common local credential files, regardless of
# which shell command is used to read them (cat, python, curl, etc).
set -euo pipefail

input="$(cat)"
cmd="$(jq -r '.tool_input.command // empty' <<<"$input")"

[ -z "$cmd" ] && exit 0

deny() {
  jq -n --arg reason "$1" '{
    hookSpecificOutput: {
      hookEventName: "PreToolUse",
      permissionDecision: "deny",
      permissionDecisionReason: $reason
    }
  }'
  exit 0
}

# Bare env/printenv/set dumps as a standalone statement (start/;/&&/||/| ... end/;/&&/||/|).
# Does NOT match "env FOO=bar cmd" (legit override) or "set -e" (legit shell option).
if grep -qE '(^|[;&|]) *(env|printenv|set) *($|[;&|])' <<<"$cmd"; then
  deny "Blocked: bare env/printenv/set dump can expose live secrets (tokens, keys) to the model backend. Use a scoped check instead, e.g. 'echo \$SPECIFIC_VAR' for a single named variable."
fi

# References to common local credential files/dirs, wherever they appear
# in the command (covers cat/less/head/tail/python/curl/base64/etc).
credential_pattern='(\.ssh/|\.aws/credentials|\.aws/config\b|\.npmrc\b|\.pypirc\b|\.docker/config\.json|\.git-credentials\b|\.(pem|pfx|key)([^a-zA-Z0-9_.]|$))'
if grep -qE "$credential_pattern" <<<"$cmd"; then
  deny "Blocked: command references a local credential file/dir (~/.ssh, ~/.aws, ~/.npmrc, ~/.pypirc, ~/.docker/config.json, .git-credentials, *.pem/*.pfx/*.key). These must never be read into model context."
fi

exit 0
