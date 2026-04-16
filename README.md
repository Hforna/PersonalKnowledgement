# PersonalKnowledge

PersonalKnowledge is a powerful .NET 8 application designed to process documents, generate AI-driven embeddings, and store them in a vector database for advanced semantic search. It provides an automated pipeline for uploading, parsing, and indexing your knowledge base.

## 🚀 Key Features

*   **Multi-Storage Support**: Support for both Local File Storage and Azure Blob Storage.
*   **PDF Parsing**: Extracts text from PDF documents using `PdfPig`.
*   **Intelligent Chunking**: Automatically splits large documents into optimized text chunks for embedding generation.
*   **AI-Powered Embeddings**: Integrates with **OpenAI** and **Azure OpenAI** (`text-embedding-3-small`) to generate high-quality vector representations of text.
*   **Vector Database Integration**: Uses **Qdrant** to store and query embeddings, enabling fast and accurate semantic search.
*   **Background Processing**: Powered by **Hangfire**, ensuring document processing is handled efficiently in the background without blocking the user interface.
*   **Clean Architecture**: Well-structured solution following domain-driven design principles.

## 🛠️ Tech Stack

*   **Backend**: .NET 8, ASP.NET Core Web API
*   **Job Orchestration**: Hangfire
*   **Database**: SQL Server (Entity Framework Core)
*   **Vector Database**: Qdrant
*   **AI SDK**: OpenAI / Azure OpenAI (v2.x)
*   **PDF Extraction**: PdfPig
*   **Storage**: Azure Blob Storage / Local File System
*   **Logging**: Serilog

## 📂 Project Structure

*   **PersonalKnowledge**: The main Web API project, controllers, and application startup logic.
*   **PersonalKnowledge.Application**: Application services, file handling, and DTOs.
*   **PersonalKnowledge.Infrastructure**: Data persistence (EF Core), external service integrations (OpenAI, Qdrant, Storage), and Hangfire job implementations.
*   **PersonalKnowledge.Domain**: Core entities, interfaces, and exceptions.
*   **PersonalKnowledge.Workers**: Configuration for Hangfire background workers.

## ⚙️ Configuration

The application requires several services to be configured in `appsettings.json` or `appsettings.Development.json`.

### SQL Server
Configure the connection string for the main database:
```json
{
  "ConnectionStrings": {
    "sqlserver": "Server=localhost,1433;Database=PersonalKnowledge;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
  }
}
```

### OpenAI / Azure OpenAI
Set up your AI provider. Support for both standard OpenAI and Azure OpenAI is included:
```json
{
  "services": {
    "openai": {
      "ApiKey": "your-api-key",
      "Endpoint": "https://your-endpoint.openai.azure.com/",
      "ChatModel": "gpt-4o",
      "EmbeddingModel": "text-embedding-3-small",
      "IsAzureOpenAI": true
    }
  }
}
```

### Qdrant
Provide the connection string for your Qdrant instance:
```json
{
  "ConnectionStrings": {
    "qdrant": "http://localhost:6344"
  }
}
```

### Storage
Choose between `Local` or `Azure` storage:
```json
{
  "Storage": {
    "Type": "Local",
    "LocalPath": "./uploads"
  }
}
```

## 🏃 Getting Started

1.  **Clone the repository**.
2.  **Update Configuration**: Set up your connection strings and API keys in `appsettings.Development.json`.
3.  **Database Migration**: Run EF Core migrations to set up the SQL Server database.
    ```bash
    dotnet ef database update --project PersonalKnowledge.Infrastructure --startup-project PersonalKnowledge
    ```
4.  **Run the Application**:
    ```bash
    dotnet run --project PersonalKnowledge
    ```
5.  **Access Swagger**: Once running, navigate to `https://localhost:<port>/swagger` to explore and test the API endpoints.

## 🧹 Background Jobs

The application uses Hangfire to process documents. On every startup, the system automatically clears any pending, scheduled, or interrupted jobs to ensure a clean processing state. You can monitor jobs via the Hangfire Dashboard (if configured) or through the application logs.

---
*Developed with .NET 8 and modern AI integration.*
