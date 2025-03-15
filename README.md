# FlowStorage Library

FlowStorage es una librería desarrollada en .NET 8 que proporciona una abstracción para gestionar el almacenamiento de datos en flujos ("flow storage").  
La librería permite trabajar tanto con almacenamiento local (sistema de ficheros) como con Azure Blob Storage, ofreciendo una solución flexible y escalable para aplicaciones distribuidas.

## Características

- **Abstracciones y Contratos**  
  Define interfaces que permiten interactuar con el almacenamiento sin depender de implementaciones específicas.

- **Soporte para Múltiples Almacenamientos**  
  - **LocalFlowStorage**: Implementación basada en el sistema de ficheros local.  
  - **AzureBlobFlowStorage**: Implementación que utiliza Azure Blob Storage (integración con wrappers para facilitar testing y abstraer el SDK).

- **Patrón Factory**  
  La clase `FlowStorageFactory` permite crear la implementación correcta de `IFlowStorage` en función de la configuración (mediante enumerados y variables de entorno).

- **Integración con Dependency Injection (DI)**  
  Extensión `AddFlowStorage` para registrar en el contenedor de dependencias solo las implementaciones necesarias según el tipo de almacenamiento configurado.

- **Tests Unitarios y de Integración**  
  Cobertura completa con tests unitarios, tests de integración (usando TestContainers y Azurite para Azure, y directorios temporales para almacenamiento local), tests de casos extremos y pruebas de concurrencia.

## Requisitos

- .NET 8 o superior
- Docker (para ejecutar los tests de integración con Azurite a través de TestContainers)

## Instalación

La librería está disponible en [NuGet.org](https://www.nuget.org). Para instalarla, simplemente añade el paquete a tu proyecto usando el siguiente comando:

Abre la solución en Visual Studio o ejecuta:
   ```bash
   dotnet add package FlowStorage
   ```

O, si prefieres utilizar el Administrador de paquetes NuGet en Visual Studio, busca "FlowStorage" y selecciona la última versión disponible.

## Configuración

La librería se configura a través de `IConfiguration` y/o variables de entorno.  
Se requiere definir al menos los siguientes valores:

- **FlowStorage:type** o la variable de entorno **FLOWSTORAGE_TYPE**  
  Define el tipo de almacenamiento a utilizar. Valores posibles:
  - `LocalStorage`
  - `AzureBlobStorage`

- **FlowStorage:connectionString** o la variable de entorno **FLOWSTORAGE_CONNECTION_STRING**  
  Cadena de conexión para el almacenamiento:
  - Para **LocalStorage**: Ruta base para los archivos.
  - Para **AzureBlobStorage**: Cadena de conexión de Azure Storage o `"UseDevelopmentStorage=true"` para desarrollo con Azurite.

Ejemplo de *appsettings.json*:

```json
{
  "FlowStorage": {
    "type": "AzureBlobStorage",
    "connectionString": "UseDevelopmentStorage=true"
  }
}
```

## Uso

### Configuración de DI

Para registrar la librería en el contenedor de dependencias, utiliza el método de extensión `AddFlowStorage`. Por ejemplo:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuLibreria.Extensions;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.AddFlowStorage(configuration);

// Ahora puedes inyectar IFlowStorage en tus servicios.
var serviceProvider = services.BuildServiceProvider();
var flowStorage = serviceProvider.GetRequiredService<IFlowStorage>();
```

### Ejemplo de Uso en Código

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

## Tests

La solución incluye una suite de tests completa:

- **Tests Unitarios:**  
  Validan la lógica de negocio, manejo de excepciones y validaciones de parámetros tanto en `LocalFlowStorage` como en `AzureBlobFlowStorage`.

- **Tests de Integración:**  
  - **LocalFlowStorage:** Utiliza directorios temporales para probar la lectura/escritura en el sistema de ficheros.  
  - **AzureBlobFlowStorage:** Se realizan tests de integración contra Azurite, levantado dinámicamente con [TestContainers](https://testcontainers.org/).

- **Tests de Concurrencia y Casos Extremos:**  
  Se simulan escenarios con operaciones concurrentes, datos muy grandes y caracteres especiales para asegurar la robustez de la librería.

Para ejecutar todos los tests:

```bash
dotnet test
```

## Contribuciones

Las contribuciones son bienvenidas. Por favor, sigue las pautas de contribución del repositorio y asegúrate de que todos los tests pasen antes de enviar un pull request.

## Licencia

[MIT License](LICENSE)

¡Gracias por usar FlowStorage! Si tienes alguna pregunta o sugerencia, no dudes en abrir un issue o contactarnos.
