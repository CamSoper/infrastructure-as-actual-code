I added a `Preview` environment to the `Stage` union in Stages.cs, and now the build is broken. Fix it.

- Stack names look like `pr-<number>`, for example `pr-1013`.
- Previews should be as cheap as possible, and never protected.
- The website banner should say which pull request it's showing.
