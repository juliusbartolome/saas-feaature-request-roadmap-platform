# FeatureFlow - Azure Deployment Guide

This guide explains how to deploy FeatureFlow to Azure using Terraform and GitHub Actions.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Azure Resource Group                              │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │              Container Apps Environment                          │    │
│  │  ┌─────────────────────┐    ┌─────────────────────┐             │    │
│  │  │   API Container     │    │   Web Container     │             │    │
│  │  │   (.NET 8)          │◄───│   (Vue.js/Nginx)    │             │    │
│  │  │   Port 8080         │    │   Port 80           │             │    │
│  │  └─────────────────────┘    └─────────────────────┘             │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │ Azure Container │  │  Azure Key      │  │ Log Analytics   │          │
│  │ Registry (ACR)  │  │  Vault          │  │ Workspace       │          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
└─────────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │  Existing PostgreSQL    │
                    │  (nexoflow-dev-rg)      │
                    └─────────────────────────┘
```

## Prerequisites

Before you begin, ensure you have the following installed:

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (v2.50+)
- [Terraform](https://www.terraform.io/downloads.html) (v1.5+)
- [Docker](https://www.docker.com/get-started) (for local builds)
- An Azure subscription with Owner or Contributor access

## Quick Start

### 1. Clone and Navigate

```bash
cd infrastructure/terraform
```

### 2. Azure Authentication

```bash
# Login to Azure
az login

# Set your subscription (replace with your subscription ID)
az account set --subscription "<your-subscription-id>"
```

### 3. Create Terraform State Container

The state is stored in the existing storage account `nexoflowtfstated89f8823`.

```bash
# Create the tfstate container if it doesn't exist
az storage container create \
  --name tfstate \
  --account-name nexoflowtfstated89f8823 \
  --auth-mode login
```

### 4. Initialize Terraform

```bash
terraform init \
  -backend-config="resource_group_name=<storage-account-resource-group>" \
  -backend-config="subscription_id=<your-subscription-id>"
```

### 5. Configure Variables

```bash
# Copy the example file
cp terraform.tfvars.example terraform.tfvars

# Edit with your values
# IMPORTANT: Never commit terraform.tfvars to version control!
```

Required variables to set:
- `postgres_connection_string` - Connection string to your existing PostgreSQL
- `jwt_secret` - Secret key for JWT tokens (min 32 characters)

### 6. Plan and Apply

```bash
# Review the plan
terraform plan

# Apply the infrastructure
terraform apply
```

### 7. Get Deployment URLs

```bash
terraform output api_url
terraform output web_url
```

## GitHub Actions Setup

The repository includes a GitHub Actions workflow for automated deployments.

### Required GitHub Secrets

Navigate to **Settings > Secrets and variables > Actions** in your repository.

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `AZURE_CREDENTIALS` | Service principal JSON | See below |
| `POSTGRES_CONNECTION_STRING` | Database connection string | Your PostgreSQL connection string |
| `JWT_SECRET` | JWT signing key | Generate: `openssl rand -base64 32` |

### Required GitHub Variables

| Variable Name | Description | Example |
|--------------|-------------|---------|
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID | `12345678-1234-1234-1234-123456789012` |
| `TFSTATE_RESOURCE_GROUP` | Resource group containing storage account | `nexoflow-dev-rg` |

### Creating Azure Service Principal

```bash
# Create service principal with Contributor access
az ad sp create-for-rbac \
  --name "github-actions-featureflow" \
  --role Contributor \
  --scopes /subscriptions/<subscription-id> \
  --sdk-auth

# The output JSON should be stored as AZURE_CREDENTIALS secret
```

The output looks like:
```json
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  "activeDirectoryEndpointUrl": "...",
  "resourceManagerEndpointUrl": "...",
  "activeDirectoryGraphResourceId": "...",
  "sqlManagementEndpointUrl": "...",
  "galleryEndpointUrl": "...",
  "managementEndpointUrl": "..."
}
```

### Grant Additional Permissions

The service principal needs additional permissions for ACR and Key Vault:

```bash
# Get the service principal object ID
SP_ID=$(az ad sp list --display-name "github-actions-featureflow" --query "[0].id" -o tsv)

# After terraform apply, grant AcrPush permission
ACR_ID=$(terraform output -raw acr_id)
az role assignment create \
  --assignee $SP_ID \
  --role AcrPush \
  --scope $ACR_ID
```

### Triggering Deployments

Deployments are triggered automatically on:
- Push to `main` branch
- Manual trigger via GitHub Actions UI (workflow_dispatch)

## Resource Details

### Container Apps (Cost-Optimized for Dev)

| Setting | API | Web |
|---------|-----|-----|
| CPU | 0.25 vCPU | 0.25 vCPU |
| Memory | 0.5 Gi | 0.5 Gi |
| Min Replicas | 0 (scale to zero) | 0 |
| Max Replicas | 1 | 1 |
| Port | 8080 | 80 |

### Estimated Monthly Cost (Dev Environment)

| Resource | Estimated Cost |
|----------|---------------|
| Container Apps | $5-15 (pay per use) |
| Container Registry (Basic) | $5 |
| Key Vault | <$1 |
| Log Analytics | $0-5 |
| **Total** | **$10-25/month** |

*Note: Container Apps in consumption tier charges only for actual usage. Cost scales to near-zero when idle.*

## Terraform Files Reference

| File | Purpose |
|------|---------|
| `main.tf` | Provider config, resource group, Container Apps Environment, Managed Identity |
| `variables.tf` | Input variables with descriptions and validations |
| `outputs.tf` | Output values (URLs, resource IDs, helpful commands) |
| `backend.tf` | Azure Blob Storage backend configuration |
| `acr.tf` | Azure Container Registry with role assignments |
| `keyvault.tf` | Key Vault with secrets for database and JWT |
| `container-apps.tf` | API and Web Container Apps with configurations |
| `terraform.tfvars.example` | Example variable values (copy to terraform.tfvars) |

## Common Operations

### View Container Logs

```bash
# API logs
az containerapp logs show \
  -n ca-featureflow-api-dev \
  -g rg-featureflow-dev \
  --follow

# Web logs
az containerapp logs show \
  -n ca-featureflow-web-dev \
  -g rg-featureflow-dev \
  --follow
```

### Restart Containers

```bash
# Get current revision name
az containerapp revision list \
  -n ca-featureflow-api-dev \
  -g rg-featureflow-dev \
  --query "[0].name" -o tsv

# Restart the revision
az containerapp revision restart \
  -n ca-featureflow-api-dev \
  -g rg-featureflow-dev \
  --revision <revision-name>
```

### Scale Containers

```bash
# Scale API to 2 replicas
az containerapp update \
  -n ca-featureflow-api-dev \
  -g rg-featureflow-dev \
  --min-replicas 1 \
  --max-replicas 2
```

### Update Secrets

```bash
# Update JWT secret in Key Vault
az keyvault secret set \
  --vault-name <key-vault-name> \
  --name jwt-secret \
  --value "new-secret-value"

# Restart containers to pick up new secret
az containerapp revision restart ...
```

## Troubleshooting

### Terraform Init Fails

**Error**: `Error configuring the backend "azurerm"`

**Solution**: Ensure the storage container exists and you have access:
```bash
az storage container create --name tfstate --account-name nexoflowtfstated89f8823
```

### Container App Won't Start

**Error**: Container keeps restarting

**Solutions**:
1. Check logs: `az containerapp logs show -n <app-name> -g <rg-name>`
2. Verify the container image exists in ACR
3. Check Key Vault secret access
4. Verify database connectivity

### Database Connection Fails

**Error**: Connection refused or timeout

**Solutions**:
1. Verify PostgreSQL firewall allows Azure services
2. Check connection string format
3. Ensure SSL mode is set correctly for Azure PostgreSQL

### GitHub Actions Fails

**Error**: Azure login failed

**Solutions**:
1. Verify `AZURE_CREDENTIALS` secret is valid JSON
2. Check service principal hasn't expired
3. Ensure required permissions are granted

## Security Considerations

1. **Secrets Management**
   - All secrets are stored in Azure Key Vault
   - Container Apps access secrets via managed identity
   - Never commit `terraform.tfvars` to version control

2. **Network Security**
   - HTTPS enforced on all endpoints
   - Container Apps use managed SSL certificates
   - Database should use private endpoints in production

3. **Access Control**
   - Use Azure RBAC for resource access
   - Limit service principal permissions
   - Rotate secrets regularly

## Production Recommendations

For production deployments, consider:

1. **Scale Settings**: Increase `min_replicas` to 1+ for high availability
2. **Resources**: Increase CPU/memory allocation
3. **Database**: Use Azure Database for PostgreSQL with private endpoints
4. **Monitoring**: Set up Azure Monitor alerts and dashboards
5. **Backups**: Enable point-in-time restore for the database
6. **Custom Domain**: Configure custom domain with managed certificate
7. **WAF**: Add Azure Front Door or Application Gateway for WAF protection

## Support

For issues with:
- **Terraform**: Check [Terraform Azure Provider docs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- **Container Apps**: Check [Azure Container Apps docs](https://learn.microsoft.com/en-us/azure/container-apps/)
- **GitHub Actions**: Check workflow logs in the Actions tab
