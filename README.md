# AuroraLib.Core.Format
AuroraLib.Core.Format is an infrastructure library for defining and fast detecting custom file formats using stream signatures, content match, MIME types, and extensions.
It provides interfaces and utilities to define and recognize file formats based on stream content and file extensions.

**This package does not include any predefined file formats.**  
It is intended as a foundation for libraries and applications that implement their own format definitions.


[![NuGet](https://img.shields.io/nuget/v/AuroraLib.Core.Format.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/AuroraLib.Core.Format)

## How To Use

### Add a new Format
You can define a custom format by implementing `IFormatInfoProvider` with `FormatInfo<T>` and detection logic in `IsMatch`.  
Alternatively, you can add simple formats directly using `FormatInfo` without creating a provider class.

``` csharp
public sealed class MyFormat : IFormatInfoProvider
{
    // Format metadata
    public IFormatInfo Info { get; } = new FormatInfo<MyFormat>(
        fullName: "My Custom Format",
        mediaType: new MediaType(MIMEType.Application, "myformat"), // => "application/myformat"
        fileExtensions: new[] { ".myf" },
        identifier: new Identifier32(0x4D594631), // => MYF1"
        identifierOffset: 0
    );

    // Detection logic
    public bool IsMatch(Stream stream, ReadOnlySpan<char> fileNameAndExtension = default)
        => IsMatchStatic(stream, fileNameAndExtension);

    public static bool IsMatchStatic(Stream stream, ReadOnlySpan<char> fileNameAndExtension = default)
    {
        // Custom detection logic goes here
        // Example: check first bytes of the stream for magic number
        return true;
    }
}
```
Define a simple PNG format without implementing IFormatProvider
This is useful for external formats, formats that only need signature/extension recognition, or formats that don´t require custom logic.

``` csharp
// Define a simple PNG format
var pngFormat = new FormatInfo(
    fullName: "PNG Image",
    mediaType: new MediaType(MIMEType.Image, "png"), // => image/png
    fileExtension: ".png",
    identifier: new Identifier64(0x89504E470D0A1A0A), // PNG magic bytes
    identifierOffset: 0
);
```

### Register Formats in a FormatDictionary
You can either register formats manually or scan assemblies for all IFormatProvider implementations.
``` csharp
// Automatically registers all IFormatProvider implementations in a assembly.
var dictionary = new FormatDictionary(typeof(MyFormat).Assembly);
```

``` csharp
// Create a FormatDictionary and add the format
var dictionary = new FormatDictionary();
dictionary.Add(pngFormat);
```

### Identify a format
Detect the format of a file or stream.
``` csharp
string fileName = "example.myf";
using var stream = File.OpenRead(fileName);

// Attempt to identify the format
if (dictionary.Identify(stream, fileName.AsSpan(), out var format))
{
    Console.WriteLine($"Detected format: {format.FullName}");
    Console.WriteLine($"MIME type: {format.MIMEType}");
    Console.WriteLine($"Extensions: {string.Join(", ", format.FileExtensions)}");

    if (format.Class != null)
    {
        // Optionally create an instance of the detected format and process the file using your custom logic
	     var instance = format.CreateInstance();
    }
}
else
{
    Console.WriteLine("Unknown format");
}
```
FormatDictionary allows you to quickly retrieve a registered format by its MIME type or file extension using TryGetValue.
``` csharp
// Lookup by MIME type
if (dictionary.TryGetValue("image/png", out IFormatInfo? pngFormat))
{
    Console.WriteLine($"Found format: {pngFormat.FullName}");
}
```
