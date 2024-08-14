# Releasing the package

1. Open a PR to update the `<VersionPrefix>` tag in [Directory.Build.Props](./Directory.Build.props)
2. Maintainers approve and merge PR
3. After the PR merges a maintainer triggers the Release workflow on the main branch via the github Actions UI or runs:

```
gh workflow run build -f test-run=false
```

4. The release flow will build and publish the packages to nuget, tag the repository with version of the release, and create a GH release
5. Maintainer updates release notes with package info:

```
# https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.Wasm.SDK
dotnet add package BytecodeAlliance.Componentize.DotNet.Wasm.SDK

# https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.WitBindgen
dotnet add package BytecodeAlliance.Componentize.DotNet.WitBindgen
```