
# FlowStorage

**FlowStorage is free, but powered by [your donations](https://github.com/sponsors/poldelahoz)**

[![NuGet Version](https://img.shields.io/nuget/vpre/FlowStorage)](https://www.nuget.org/packages/FlowStorage)
![NuGet Downloads](https://img.shields.io/nuget/dt/FlowStorage)
![GitHub License](https://img.shields.io/github/license/poldelahoz/FlowStorage)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/691e960f4e1a45e58e3556acaf3adf5c)](https://app.codacy.com/gh/poldelahoz/FlowStorage/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

FlowStorage is a library developed in .NET 8 that provides an abstraction for managing data storage.
The library supports both local storage (file system) and Azure Blob Storage, offering a flexible and scalable solution for distributed applications.

## Supported platforms (adding more soon)

- **In Memory Storage** (Just for test purpose, produces high memory fragmentation)
- **Local File System**
- **Azure Blob Storage**

## Requirements

- .NET 8 or higher

## Installation

The library is available on [NuGet.org](https://www.nuget.org/packages/FlowStorage). To install it, simply add the package to your project using the following command:

In CLI:
```bash
dotnet add package FlowStorage
```

Or, if you prefer to use the NuGet Package Manager in Visual Studio, search for "FlowStorage" and select the latest version available.

## Configuration

Use `appsettings.json` or environment variables to configure the storage.  

- **FlowStorage:type** or environment variable **FLOWSTORAGE_TYPE**  
  Defines the type of storage to use. Possible values:
  - `LocalStorage`
  - `AzureBlobStorage`
  - `InMemoryStorage`

- **FlowStorage:connectionString** or environment variable **FLOWSTORAGE_CONNECTION_STRING**  
  - For **LocalStorage**: Base path for files.
  - For **AzureBlobStorage**: Azure Storage connection string
  - For **InMemoryStorage**: ignored

Example of *appsettings.json*:

```json
{
  "FlowStorage": {
    "type": "AzureBlobStorage",
    "connectionString": "AccountName={your_account_name};AccountKey={your_account_key};EndpointSuffix={your_endpoint_suffix}"
  }
}
```

## Quick start

### DI configuration

To register the library in the dependency container, use the `AddFlowStorage` extension method.

```csharp
builder.Services.AddFlowStorage(builder.Configuration);
```

### Example of use

```csharp
public class MyStorageService
{
    private readonly IFlowStorage _flowStorage;

    public MyStorageService(IFlowStorage flowStorage)
    {
        _flowStorage = flowStorage;
    }

    public async Task UploadAndDownloadFile(string container, string fileName, string content)
    {
        await _flowStorage.CreateContainerIfNotExistsAsync(container);
        await _flowStorage.UploadFileAsync(container, fileName, content);
        var downloadedContent = await _flowStorage.ReadFileAsync(container, fileName);
        Console.WriteLine($"Downloaded content: {downloadedContent}");
    }
}
```

## Contributions

Contributions are welcome. Please follow the repository's contribution guidelines and ensure all tests pass before submitting a pull request.

## License

[MIT License](LICENSE)

Thank you for using FlowStorage! If you have any questions or suggestions, please feel free to open an issue or contact me.
