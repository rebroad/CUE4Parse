# External build tree

- Keep source edits in this repository and build in `/var/tmp/builds/rebroad/src/CUE4Parse.build`.
- Sync the populated build tree with `cpto --lngit -r <source> <build>` when build tools need Git metadata; this gives the target its own linked-worktree metadata and recursively syncs initialized submodules. Do not symlink the source `.git` directory into the build tree.
- Review target-only cleanup before using `--lngit`: it removes non-ignored target-only files. Preserve any target data that must remain.
- When Git source metadata is unnecessary, keep the build tree unlinked and pass `-p:EnableSourceControlManagerQueries=false` to `dotnet build`; this disables Source Link's MSBuild Git query for that build without changing package settings.
