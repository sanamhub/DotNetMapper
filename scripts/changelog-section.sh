#!/usr/bin/env bash
# Prints one version's section from CHANGELOG.md.
#
# The release notes are built from this rather than from the commit log. A generated commit list
# is a record of how the work happened, which is not what someone opening a release page wants to
# read. The changelog is already written for them, and sourcing the notes from it means the two
# cannot say different things.
set -euo pipefail

VERSION="${1:-}"
FILE="${2:-CHANGELOG.md}"

[ -n "$VERSION" ] || { echo "usage: changelog-section.sh <version> [changelog]" >&2; exit 2; }
[ -f "$FILE" ] || { echo "no such file: $FILE" >&2; exit 1; }

# Literal prefix comparison, not a regex. A version string is full of dots and dashes, and
# "[0.1.0-beta.1]" read as a pattern is a character class whose 0-b range covers most of the
# alphabet, so it matched the Unreleased heading as well.
section="$(awk -v head="## [$VERSION]" '
  index($0, head) == 1 { found = 1; next }
  found && index($0, "## ") == 1 { exit }
  found { print }
' "$FILE")"

# Trim leading and trailing blank lines.
section="$(sed -e '/./,$!d' <<< "$section" | sed -e :a -e '/^\n*$/{$d;N;};/\n$/ba')"

if [ -z "$section" ]; then
  echo "CHANGELOG.md has no section for $VERSION" >&2
  echo "A release without notes is a release nobody can read. Add the entry first." >&2
  exit 1
fi

printf '%s\n' "$section"
