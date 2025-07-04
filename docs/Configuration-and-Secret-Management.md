# Configuration and Secret Management Guide

## Overview

This document outlines the configuration and secret management implementation in the Bwadl project, following .NET best practices for security, maintainability, and environment-specific deployments.

## Configuration Architecture

### 1. Configuration Hierarchy

The application follows .NET's configuration hierarchy (later sources override earlier ones):

1. **appsettings.json** - Base configuration for all environments
2. **appsettings.{Environment}.json** - Environment-specific overrides
3. **User Secrets** - Development secrets (never committed to source control)
4. **Environment Variables** - Production configuration and secrets
5. **Command Line Arguments** - Runtime overrides

### 2. Strongly-Typed Configuration

All configuration is mapped to strongly-typed classes in `Bwadl.Shared.Configuration`:

- `ApplicationOptions` - Application metadata
- `CacheOptions` - Caching configuration
- `MessageBusOptions` - Message bus settings
- `SecurityOptions` - Security and JWT settings
- `FeatureOptions` - Feature flags

### 3. Configuration Service

The `IConfigurationService` provides a unified interface for accessing both regular configuration and secrets:

```csharp
// Get regular configuration
var cacheOptions = await configService.GetOptionsAsync<CacheOptions>();

// Get configuration with secrets
var securityOptions = await configService.GetSecurityOptionsWithSecretsAsync();

// Get individual secrets
var apiKey = await configService.GetSecretConfigurationAsync("ExternalServices:EmailService:ApiKey");
```

## Secret Management

### 1. Secret Manager Interface

The `ISecretManager` interface provides a consistent API for secret retrieval across different providers:

```csharp
public interface ISecretManager
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
}
```

### 2. Secret Resolution Order

The secret manager attempts to retrieve secrets in this order:

1. **Environment Variables** - For containerized/cloud deployments
2. **User Secrets** - For local development
3. **Cloud Providers** - Azure Key Vault, AWS Secrets Manager, etc.
4. **Default Values** - Development fallbacks (with warnings)

### 3. Development Setup

For local development, use .NET User Secrets:

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Jwt:SecretKey" "your-jwt-secret"
dotnet user-secrets set "MessageBus:RabbitMq:Password" "your-password"
dotnet user-secrets set "ExternalServices:EmailService:ApiKey" "your-api-key"

# List all secrets
dotnet user-secrets list
```

### 4. Production Deployment

For production, use environment variables or cloud secret providers:

```bash
# Environment variables (Docker/Kubernetes)
export Jwt__SecretKey="production-jwt-secret"
export MessageBus__RabbitMq__Password="production-password"
export ExternalServices__EmailService__ApiKey="production-api-key"
```

Note: Use double underscores (`__`) in environment variables to represent configuration hierarchy.

## Cloud Provider Integration

### Azure Key Vault

To integrate with Azure Key Vault, install the package and update the `SecretManager`:

```bash
dotnet add package Azure.Security.KeyVault.Secrets
dotnet add package Azure.Identity
```

```csharp
private async Task<string?> GetFromCloudProviderAsync(string secretName, CancellationToken cancellationToken)
{
    var keyVaultUrl = _configuration["KeyVault:Url"];
    if (string.IsNullOrEmpty(keyVaultUrl))
        return null;

    var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    var secret = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
    return secret.Value.Value;
}
```

### AWS Secrets Manager

For AWS Secrets Manager:

```bash
dotnet add package AWSSDK.SecretsManager
```

```csharp
private async Task<string?> GetFromCloudProviderAsync(string secretName, CancellationToken cancellationToken)
{
    var client = new AmazonSecretsManagerClient();
    var request = new GetSecretValueRequest { SecretId = secretName };
    var response = await client.GetSecretValueAsync(request, cancellationToken);
    return response.SecretString;
}
```

### HashiCorp Vault

For HashiCorp Vault:

```bash
dotnet add package VaultSharp
```

## Best Practices

### 1. Secret Naming Conventions

- Use hierarchical naming: `Service:Component:Secret`
- Examples: `Jwt:SecretKey`, `Database:ConnectionString`, `ExternalServices:EmailService:ApiKey`

### 2. Environment-Specific Configuration

- Keep sensitive data out of `appsettings.json`
- Use environment-specific files for non-secret environment differences
- Use secrets management for sensitive data

### 3. Configuration Validation

- Use strongly-typed configuration classes
- Implement validation in the configuration classes
- Fail fast on startup if required configuration is missing

### 4. Logging and Monitoring

- Log configuration retrieval (without sensitive values)
- Monitor secret access patterns
- Alert on secret retrieval failures

### 5. Security Considerations

- Never log secret values
- Use secure transport (HTTPS/TLS) for secret retrieval
- Implement proper access controls
- Rotate secrets regularly
- Use different secrets for different environments

## Tools and SDKs

### 1. Built-in .NET Tools

- **Configuration API** - Built-in hierarchical configuration system
- **Options Pattern** - Strongly-typed configuration binding
- **User Secrets** - Development secret storage

### 2. Cloud Provider SDKs

- **Azure Key Vault** - `Azure.Security.KeyVault.Secrets`
- **AWS Secrets Manager** - `AWSSDK.SecretsManager`
- **Google Secret Manager** - `Google.Cloud.SecretManager.V1`
- **HashiCorp Vault** - `VaultSharp`

### 3. Container Orchestration

- **Kubernetes Secrets** - Native secret management
- **Docker Secrets** - Docker Swarm secret management
- **Helm** - Chart-based configuration management

### 4. Configuration Management Tools

- **Consul** - Distributed configuration
- **etcd** - Distributed key-value store
- **ConfigMap** - Kubernetes configuration

## Example Usage

### In Controllers

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IConfigurationService _configService;

    public UsersController(IConfigurationService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var features = await _configService.GetOptionsAsync<FeatureOptions>();
        
        if (!features.EnableAnalytics)
        {
            // Skip analytics if disabled
        }
        
        // ... rest of implementation
    }
}
```

### In Services

```csharp
public class EmailService : IEmailService
{
    private readonly ISecretManager _secretManager;

    public EmailService(ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var apiKey = await _secretManager.GetSecretAsync("ExternalServices:EmailService:ApiKey");
        // Use the API key to send email
    }
}
```

## Deployment Considerations

### 1. Docker

```dockerfile
# Use build arguments for non-secret configuration
ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ENVIRONMENT

# Secrets should be provided via environment variables or mounted volumes
# Never include secrets in the Docker image
```

### 2. Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: bwadl-secrets
type: Opaque
data:
  jwt-secret-key: <base64-encoded-secret>
  
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bwadl-api
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: bwadl-secrets
              key: jwt-secret-key
```

### 3. Azure App Service

Use Application Settings and Key Vault references:

```json
{
  "Jwt:SecretKey": "@Microsoft.KeyVault(VaultName=my-vault;SecretName=jwt-secret-key)"
}
```

## Testing Configuration

### 1. Unit Tests

```csharp
[Test]
public async Task ConfigurationService_Should_Return_Valid_Options()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.test.json")
        .Build();
    
    var secretManager = new Mock<ISecretManager>();
    var configService = new ConfigurationService(configuration, secretManager.Object, serviceProvider);
    
    // Act
    var options = await configService.GetOptionsAsync<CacheOptions>();
    
    // Assert
    Assert.That(options.DefaultExpirationMinutes, Is.GreaterThan(0));
}
```

### 2. Integration Tests

```csharp
[Test]
public async Task Should_Retrieve_Secrets_From_Test_Provider()
{
    // Use test-specific configuration and mock secret providers
}
```

This configuration and secret management setup provides a secure, maintainable, and scalable foundation for your .NET application across all environments.
