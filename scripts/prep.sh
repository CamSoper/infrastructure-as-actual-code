#!/usr/bin/env bash
# Gets every demo ready to present: builds, tests, creates stacks, pre-deploys the sites.
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

step 'Demo 2: dev stack (deployed) and friday stack (not deployed)'
ensure_stack demos/02-compiler-as-policy dev
deploy demos/02-compiler-as-policy dev
ensure_stack demos/02-compiler-as-policy friday

step 'Demo 3: pr-1013 stack (pre-deployed with the reference solution, then reset)'
# Pre-creating the storage account means the live `pulumi up` only replaces one file.
ensure_stack demos/03-agent-vs-compiler pr-1013
demo3="$root/demos/03-agent-vs-compiler"
reset_demo3() { (cd "$demo3" && git checkout -- .); }
cd "$demo3"
git apply --ignore-whitespace fallback/preview-environment.patch \
    || { echo 'Could not apply the fallback patch. Is demos/03 already modified? Run git checkout -- . first.' >&2; exit 1; }
trap reset_demo3 EXIT
# blame=prep gets past the Friday rule if you're prepping on a Friday afternoon. Removed right after.
status=0
pulumi up --stack pr-1013 --yes --skip-preview --config blame=prep || status=$?
pulumi config rm blame --stack pr-1013
[ "$status" -eq 0 ] || { echo 'pulumi up failed for pr-1013' >&2; exit "$status"; }
pulumi stack output url --stack pr-1013

printf '\n\033[32mReady. Open the URLs above in browser tabs.\033[0m\n'
