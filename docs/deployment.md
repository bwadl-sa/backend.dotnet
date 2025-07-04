# Deployment Guide

## Overview

This guide covers deploying the Bwadl enterprise .NET Core application across different environments (Development, Staging, Production) using various deployment strategies including Docker containers, Kubernetes, and cloud platforms.

## Prerequisites

### Required Tools
- **Docker**: For containerization
- **Kubernetes CLI (kubectl)**: For Kubernetes deployments
- **Helm**: For Kubernetes package management
- **Azure CLI** or **AWS CLI**: For cloud deployments
- **.NET 8.0 SDK**: For building the application

### Required Services
- **SQL Server**: Primary database
- **Redis**: Caching and session storage
- **Application Insights** or **Grafana**: Monitoring and logging
- **Key Vault**: Secret management
- **Load Balancer**: For high availability

## Environment Configuration

### Development Environment

#### Local Development Setup

```bash
# Clone and setup
git clone https://github.com/your-org/bwadl.git
cd bwadl

# Configure user secrets
cd src/Bwadl.API
dotnet user-secrets set "Security:Jwt:SecretKey" "your-development-secret-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=BwadlDb_Dev;Trusted_Connection=true"

# Run with watch
dotnet watch run
```

#### Docker Development

```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/Bwadl.API/Dockerfile
      target: development
    ports:
      - "5281:8080"
      - "7260:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+:8081;http://+:8080
      - ASPNETCORE_HTTPS_PORT=7260
    volumes:
      - ~/.aspnet/https:/root/.aspnet/https:ro
      - ~/.microsoft/usersecrets:/root/.microsoft/usersecrets:ro
    depends_on:
      - sqlserver
      - redis

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  sqlserver_data:
  redis_data:
```

### Staging Environment

#### Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "Security": {
    "Jwt": {
      "Issuer": "Bwadl.API.Staging",
      "Audience": "Bwadl.Client.Staging",
      "ExpirationMinutes": 60
    },
    "ApiKeys": {
      "RequireApiKey": true
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "#{STAGING_CONNECTION_STRING}#"
  },
  "Caching": {
    "Redis": {
      "ConnectionString": "#{STAGING_REDIS_CONNECTION}#",
      "InstanceName": "Bwadl.Staging"
    }
  },
  "Monitoring": {
    "ApplicationInsights": {
      "InstrumentationKey": "#{STAGING_AI_KEY}#"
    },
    "HealthChecks": {
      "Enabled": true,
      "DetailedErrors": true
    }
  }
}
```

#### Docker Compose for Staging

```yaml
# docker-compose.staging.yml
version: '3.8'

services:
  api:
    image: bwadl-api:staging
    build:
      context: .
      dockerfile: src/Bwadl.API/Dockerfile
      target: runtime
    ports:
      - "80:8080"
      - "443:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
      - ASPNETCORE_URLS=https://+:8081;http://+:8080
    env_file:
      - .env.staging
    depends_on:
      - redis
    networks:
      - bwadl-network
    deploy:
      replicas: 2
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3

  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes
    volumes:
      - redis_staging_data:/data
    networks:
      - bwadl-network
    deploy:
      restart_policy:
        condition: on-failure

networks:
  bwadl-network:
    driver: bridge

volumes:
  redis_staging_data:
```

### Production Environment

#### Configuration Management

Production configuration should use environment variables and secret management:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Bwadl": "Information"
    }
  },
  "Security": {
    "Jwt": {
      "Issuer": "Bwadl.API.Production",
      "Audience": "Bwadl.Client.Production",
      "ExpirationMinutes": 30
    },
    "ApiKeys": {
      "RequireApiKey": true
    }
  },
  "Monitoring": {
    "ApplicationInsights": {
      "InstrumentationKey": "#{PRODUCTION_AI_KEY}#"
    },
    "HealthChecks": {
      "Enabled": true,
      "DetailedErrors": false
    }
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  }
}
```

## Docker Deployment

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Bwadl.API/Bwadl.API.csproj", "src/Bwadl.API/"]
COPY ["src/Bwadl.Application/Bwadl.Application.csproj", "src/Bwadl.Application/"]
COPY ["src/Bwadl.Domain/Bwadl.Domain.csproj", "src/Bwadl.Domain/"]
COPY ["src/Bwadl.Infrastructure/Bwadl.Infrastructure.csproj", "src/Bwadl.Infrastructure/"]
COPY ["src/Bwadl.Shared/Bwadl.Shared.csproj", "src/Bwadl.Shared/"]

# Restore dependencies
RUN dotnet restore "src/Bwadl.API/Bwadl.API.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/src/Bwadl.API"
RUN dotnet build "Bwadl.API.csproj" -c Release -o /app/build
RUN dotnet publish "Bwadl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Development stage
FROM build AS development
WORKDIR /src/src/Bwadl.API
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "https://+:8081;http://+:8080"]

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 dotnet \
    && adduser --system --uid 1001 --ingroup dotnet dotnet

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R dotnet:dotnet /app
USER dotnet

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Bwadl.API.dll"]
```

### Multi-stage Build Script

```bash
#!/bin/bash
# build-and-deploy.sh

set -e

# Configuration
REGISTRY="your-registry.azurecr.io"
IMAGE_NAME="bwadl-api"
VERSION=${1:-latest}
ENVIRONMENT=${2:-staging}

echo "Building Bwadl API v${VERSION} for ${ENVIRONMENT}"

# Build the image
docker build \
  --target runtime \
  --tag ${REGISTRY}/${IMAGE_NAME}:${VERSION} \
  --tag ${REGISTRY}/${IMAGE_NAME}:${ENVIRONMENT}-latest \
  .

# Push to registry
docker push ${REGISTRY}/${IMAGE_NAME}:${VERSION}
docker push ${REGISTRY}/${IMAGE_NAME}:${ENVIRONMENT}-latest

echo "Successfully built and pushed ${REGISTRY}/${IMAGE_NAME}:${VERSION}"
```

## Kubernetes Deployment

### Namespace and Configuration

```yaml
# namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: bwadl-production
  labels:
    name: bwadl-production
    environment: production

---
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: bwadl-config
  namespace: bwadl-production
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  Security__Jwt__Issuer: "Bwadl.API.Production"
  Security__Jwt__Audience: "Bwadl.Client.Production"
  Security__Jwt__ExpirationMinutes: "30"
  Caching__Redis__InstanceName: "Bwadl.Production"

---
# secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: bwadl-secrets
  namespace: bwadl-production
type: Opaque
data:
  # Base64 encoded values
  connectionstring: <base64-encoded-connection-string>
  jwt-secret: <base64-encoded-jwt-secret>
  redis-connection: <base64-encoded-redis-connection>
```

### Deployment Configuration

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bwadl-api
  namespace: bwadl-production
  labels:
    app: bwadl-api
    version: v1
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bwadl-api
  template:
    metadata:
      labels:
        app: bwadl-api
        version: v1
    spec:
      containers:
      - name: bwadl-api
        image: your-registry.azurecr.io/bwadl-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: bwadl-secrets
              key: connectionstring
        - name: Security__Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: bwadl-secrets
              key: jwt-secret
        - name: Caching__Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: bwadl-secrets
              key: redis-connection
        envFrom:
        - configMapRef:
            name: bwadl-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
        securityContext:
          allowPrivilegeEscalation: false
          runAsNonRoot: true
          runAsUser: 1001
```

### Service and Ingress

```yaml
# service.yaml
apiVersion: v1
kind: Service
metadata:
  name: bwadl-api-service
  namespace: bwadl-production
spec:
  selector:
    app: bwadl-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP

---
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: bwadl-api-ingress
  namespace: bwadl-production
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.bwadl.com
    secretName: bwadl-api-tls
  rules:
  - host: api.bwadl.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: bwadl-api-service
            port:
              number: 80
```

### Horizontal Pod Autoscaler

```yaml
# hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bwadl-api-hpa
  namespace: bwadl-production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bwadl-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## Cloud Deployment

### Azure App Service

#### ARM Template

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "webAppName": {
      "type": "string",
      "metadata": {
        "description": "Name of the web app"
      }
    },
    "environment": {
      "type": "string",
      "allowedValues": ["staging", "production"],
      "defaultValue": "staging"
    }
  },
  "variables": {
    "appServicePlanName": "[concat(parameters('webAppName'), '-plan')]",
    "appInsightsName": "[concat(parameters('webAppName'), '-ai')]"
  },
  "resources": [
    {
      "type": "Microsoft.Web/serverfarms",
      "apiVersion": "2021-02-01",
      "name": "[variables('appServicePlanName')]",
      "location": "[resourceGroup().location]",
      "sku": {
        "name": "P1v3",
        "tier": "PremiumV3",
        "size": "P1v3",
        "family": "Pv3",
        "capacity": 2
      },
      "properties": {
        "reserved": false
      }
    },
    {
      "type": "Microsoft.Web/sites",
      "apiVersion": "2021-02-01",
      "name": "[parameters('webAppName')]",
      "location": "[resourceGroup().location]",
      "dependsOn": [
        "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]"
      ],
      "properties": {
        "serverFarmId": "[resourceId('Microsoft.Web/serverfarms', variables('appServicePlanName'))]",
        "httpsOnly": true,
        "siteConfig": {
          "netFrameworkVersion": "v8.0",
          "metadata": [
            {
              "name": "CURRENT_STACK",
              "value": "dotnet"
            }
          ],
          "appSettings": [
            {
              "name": "ASPNETCORE_ENVIRONMENT",
              "value": "[parameters('environment')]"
            },
            {
              "name": "APPINSIGHTS_INSTRUMENTATIONKEY",
              "value": "[reference(resourceId('Microsoft.Insights/components', variables('appInsightsName'))).InstrumentationKey]"
            }
          ]
        }
      }
    }
  ]
}
```

### AWS ECS Deployment

#### Task Definition

```json
{
  "family": "bwadl-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "bwadl-api",
      "image": "your-account.dkr.ecr.region.amazonaws.com/bwadl-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "ASPNETCORE_URLS",
          "value": "http://+:8080"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:bwadl/db-connection"
        },
        {
          "name": "Security__Jwt__SecretKey",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:bwadl/jwt-secret"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/bwadl-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": [
          "CMD-SHELL",
          "curl -f http://localhost:8080/health || exit 1"
        ],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

## CI/CD Pipeline

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
name: Build and Deploy

on:
  push:
    branches: [main, staging]
  pull_request:
    branches: [main]

env:
  REGISTRY: your-registry.azurecr.io
  IMAGE_NAME: bwadl-api

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3

  build-and-push-image:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' || github.ref == 'refs/heads/staging'
    steps:
    - uses: actions/checkout@v4
    
    - name: Log in to registry
      uses: azure/docker-login@v1
      with:
        login-server: ${{ env.REGISTRY }}
        username: ${{ secrets.REGISTRY_USERNAME }}
        password: ${{ secrets.REGISTRY_PASSWORD }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=sha,prefix={{branch}}-
    
    - name: Build and push image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./src/Bwadl.API/Dockerfile
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}

  deploy-staging:
    needs: build-and-push-image
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/staging'
    environment: staging
    steps:
    - name: Deploy to staging
      run: |
        echo "Deploying to staging environment"
        # Add staging deployment commands

  deploy-production:
    needs: build-and-push-image
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    steps:
    - name: Deploy to production
      run: |
        echo "Deploying to production environment"
        # Add production deployment commands
```

## Monitoring and Observability

### Health Checks Configuration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql", "ready" })
    .AddRedis(
        connectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: new[] { "cache", "redis", "ready" })
    .AddCheck<CustomHealthCheck>("custom", tags: new[] { "custom" });

// Configure health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Application Insights Integration

```csharp
// Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
});

// Custom telemetry
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
```

## Deployment Checklist

### Pre-deployment
- [ ] All tests pass
- [ ] Security scan completed
- [ ] Performance testing completed
- [ ] Database migrations prepared
- [ ] Configuration validated
- [ ] Secrets configured
- [ ] Monitoring set up

### Deployment
- [ ] Blue-green deployment strategy
- [ ] Database migration executed
- [ ] Health checks passing
- [ ] Load balancer configured
- [ ] SSL certificates valid
- [ ] Monitoring alerts active

### Post-deployment
- [ ] Smoke tests pass
- [ ] Performance metrics within expected range
- [ ] Error rates normal
- [ ] User acceptance testing completed
- [ ] Documentation updated
- [ ] Team notified

## Rollback Procedures

### Quick Rollback

```bash
#!/bin/bash
# rollback.sh

ENVIRONMENT=${1:-staging}
PREVIOUS_VERSION=${2}

if [ -z "$PREVIOUS_VERSION" ]; then
    echo "Usage: ./rollback.sh <environment> <previous-version>"
    exit 1
fi

echo "Rolling back $ENVIRONMENT to version $PREVIOUS_VERSION"

# Kubernetes rollback
kubectl rollout undo deployment/bwadl-api -n bwadl-$ENVIRONMENT --to-revision=$PREVIOUS_VERSION

# Verify rollback
kubectl rollout status deployment/bwadl-api -n bwadl-$ENVIRONMENT

echo "Rollback completed successfully"
```

### Database Rollback

```bash
# Database rollback script
dotnet ef migrations remove --project src/Bwadl.Infrastructure --startup-project src/Bwadl.API
dotnet ef database update PreviousMigration --project src/Bwadl.Infrastructure --startup-project src/Bwadl.API
```

## Security Considerations

### Container Security
- Use non-root users
- Scan images for vulnerabilities
- Keep base images updated
- Implement resource limits
- Use read-only file systems where possible

### Network Security
- Enable TLS/SSL everywhere
- Use network policies in Kubernetes
- Implement proper firewall rules
- Use service mesh for microservices communication

### Secret Management
- Never store secrets in code or images
- Use dedicated secret management services
- Rotate secrets regularly
- Implement least privilege access
- Audit secret access

## Troubleshooting

### Common Issues

#### High Memory Usage
```bash
# Check memory usage in Kubernetes
kubectl top pods -n bwadl-production

# Adjust memory limits
kubectl patch deployment bwadl-api -n bwadl-production -p '{"spec":{"template":{"spec":{"containers":[{"name":"bwadl-api","resources":{"limits":{"memory":"1Gi"}}}]}}}}'
```

#### Database Connection Issues
```bash
# Test database connectivity
kubectl exec -it deployment/bwadl-api -n bwadl-production -- /bin/bash
# Inside container:
curl -X GET "https://localhost:8080/health"
```

#### Performance Issues
```bash
# Check application metrics
kubectl logs deployment/bwadl-api -n bwadl-production --tail=100
```

### Emergency Procedures

1. **Service Down**: Scale up replicas immediately
2. **Database Issues**: Implement read-only mode
3. **Security Breach**: Rotate all secrets, block suspicious IPs
4. **Performance Degradation**: Enable circuit breakers, scale horizontally

This comprehensive deployment guide ensures reliable, secure, and scalable deployment of the Bwadl enterprise application across all environments.
