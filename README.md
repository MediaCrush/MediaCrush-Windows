# MediaCrush for Windows

This is the official MediaCrush app for Windows. It sits in your taskbar and lets you upload files directly, as well as providing
a screenshot mechanism. Click the icon to upload files, or press Ctrl+PrintScreen to take a screenshot.

![](https://mediacru.sh/J9eah0xCtc3Q.png)

![](https://mediacru.sh/1XcZextWIbSx.png)

## Compiling

To compile, you should have Visual Studio 2012 or better, SharpDevelop 4.0 or better, and .NET 4.5. You can compile it from a
command line like so:

    msbuild.exe /p:Configuration=RELEASE

And you'll get binaries in `MediaCrush/bin/Release/`. You can also open up the .sln file with Visual Studio or SharpDevelop and
compile it like normal.