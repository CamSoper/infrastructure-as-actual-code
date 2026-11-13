#!/usr/bin/env bash
# Gets demos 1 and 2 ready to present: builds, tests, creates stacks, pre-deploys the sites.
# (Demo 3 is a take-home; it gets built and tested here, but not deployed.)
# Run from anywhere:  ./scripts/prep.sh
# Needs: bash (Linux, macOS, or WSL), .NET 11 SDK, Pulumi CLI (logged in), Azure CLI (az login).
# Using a local or self-managed Pulumi backend? export PULUMI_CONFIG_PASSPHRASE first.

set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

step() { printf '\n\033[36m==> %s\033[0m\n' "$1"; }

ensure_stack() { # folder stack
    (cd "$root/$1" && pulumi stack select "$2" --create)
}

deploy() { # folder stack [extra pulumi args...]
    local folder=$1 stack=$2; shift 2
    (cd "$root/$folder" && pulumi up --stack "$stack" --yes --skip-preview "$@" && pulumi stack output url --stack "$stack")
}

step 'Checking tools'
dotnet --version
pulumi version
pulumi whoami
az account show --query '{subscription:name, user:user.name}' -o table || { echo 'Run az login first.' >&2; exit 1; }

step 'Building and testing'
cd "$root"
for proj in demos/01-hello-cloud demos/02-compiler-as-policy demos/03-agent-vs-compiler; do
    dotnet build "$proj" --nologo -v quiet
done
dotnet test --project tests/StackTests

step 'Demo 1: dev stack'
ensure_stack demos/01-hello-cloud dev
deploy demos/01-hello-cloud dev

step 'Demo 2: dev stack (deployed) and prod stack (created, never deployed)'
ensure_stack demos/02-compiler-as-policy dev
deploy demos/02-compiler-as-policy dev
ensure_stack demos/02-compiler-as-policy prod

printf '\n\033[32mReady. Open the URLs above in browser tabs.\033[0m\n'
