A simple .Net app to control Windows application by keyboard

It's currently in a very basic form, just like a POC

To publish

- Update the version in csproj file
- Tag the release

```
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

- Build the exe

```
dotnet publish -c Release
```

- Then create a release on GitHub and upload the .exe
