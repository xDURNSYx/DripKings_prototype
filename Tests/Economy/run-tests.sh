#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "$0")/../.." && pwd)"
test_dir="$(mktemp -d)"
trap 'rm -rf "$test_dir"' EXIT
cp "$repo_root"/Assets/DripKings/Economy/*.cs "$test_dir/"
cp "$repo_root/Assets/DripKings/Tests/Editor/EconomyChecks.cs" "$test_dir/"
cp "$repo_root/Tests/Economy/Program.cs" "$test_dir/"
cat > "$test_dir/Economy.csproj" <<'PROJECT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup>
</Project>
PROJECT
cat > "$test_dir/NuGet.Config" <<'CONFIG'
<configuration><packageSources><clear /></packageSources></configuration>
CONFIG
"${DOTNET_BIN:-dotnet}" run --project "$test_dir/Economy.csproj"
