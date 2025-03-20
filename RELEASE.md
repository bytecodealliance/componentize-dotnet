# Releasing the package

1. Update all the tool versions in [Directory.Build.Props](./Directory.Build.props)
1. Open a PR to update:
    - the `<VersionPrefix>` tag in [Directory.Build.Props](./Directory.Build.props)
    - the `BytecodeAlliance.Componentize.DotNet.Wasm.SDK` package `version` to match the `<VersionPrefix>` in the [template](./templates/content/wasi-cli/wasi-cli.csproj). For example the template version might look like `Version="0.6.0-preview*"`. This ensures the templates use the latest package.
1. Maintainers approve and merge PR
1. After the PR merges a maintainer triggers the Release workflow on the main branch via the github Actions UI or runs:

```
gh workflow run build -f test-run=false
```

>The release flow will build and publish the packages to nuget, tag the repository with version of the release, and create a GH release
5. Maintainer updates release notes with package info:

```
# https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.Templates
dotnet new install BytecodeAlliance.Componentize.DotNet.Templates

# https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.Wasm.SDK
dotnet add package BytecodeAlliance.Componentize.DotNet.Wasm.SDK

# https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.WitBindgen
dotnet add package BytecodeAlliance.Componentize.DotNet.WitBindgen
```

6. Post message about release [c# collaboration Zulip channel](https://bytecodealliance.zulipchat.com/#narrow/channel/407028-C.23.2F.2Enet-collaboration)