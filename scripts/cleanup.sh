#!/usr/bin/env bash
# Tears down everything the demos created. Run from anywhere:  ./scripts/cleanup.sh

set -uo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

stacks=(
    "demos/01-hello-cloud dev"
    "demos/02-compiler-as-policy dev"
    "demos/02-compiler-as-policy prod"
    "demos/02-compiler-as-policy friday"
    "demos/03-agent-vs-compiler dev"
    "demos/03-agent-vs-compiler prod"
    "demos/03-agent-vs-compiler friday"
    "demos/03-agent-vs-compiler pr-1013"
)

for entry in "${stacks[@]}"; do
    read -r folder stack <<< "$entry"
    (
        cd "$root/$folder" || exit
        pulumi stack select "$stack" >/dev/null 2>&1 || exit 0
        printf '\n\033[36m==> Destroying %s in %s\033[0m\n' "$stack" "$folder"
        # Prod resources are protected on purpose. Unprotect them first, deliberately.
        pulumi state unprotect --all --yes --stack "$stack"
        # destroy works from saved state and doesn't run the program, so the Friday rule won't stop it.
        pulumi destroy --stack "$stack" --yes --skip-preview
        pulumi stack rm "$stack" --yes
    )
done

# The single-file bonus uses its own stacks (dev and prod by default).
for stack in dev prod; do
    printf '\n\033[36m==> Destroying single-file stack %s (creates and removes it if it never existed)\033[0m\n' "$stack"
    dotnet run "$root/bonus/single-file/deploy.cs" -- destroy "$stack"
done

printf '\n\033[32mDone. Double-check the Azure portal for resource groups starting with hello-cloud, policy-, agent-, or single-file-.\033[0m\n'
