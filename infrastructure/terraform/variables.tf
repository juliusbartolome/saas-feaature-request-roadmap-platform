# ============================================================================
# FeatureFlow - Terraform Variables
# ============================================================================

# ============================================================================
# General Configuration
# ============================================================================

variable "project_name" {
  type        = string
  description = "Name of the project, used for resource naming"
  default     = "featureflow"

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.project_name))
    error_message = "Project name must contain only lowercase letters, numbers, and hyphens."
  }
}

variable "environment" {
  type        = string
  description = "Environment name (e.g., dev, staging, prod)"
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  type        = string
  description = "Azure region for all resources"
  default     = "eastus"
}

# ============================================================================
# Database Configuration
# ============================================================================

variable "postgres_connection_string" {
  type        = string
  description = "Connection string for the existing PostgreSQL database"
  sensitive   = true
}

# ============================================================================
# JWT Configuration
# ============================================================================

variable "jwt_secret" {
  type        = string
  description = "Secret key for JWT token signing (min 32 characters)"
  sensitive   = true

  validation {
    condition     = length(var.jwt_secret) >= 32
    error_message = "JWT secret must be at least 32 characters long."
  }
}

variable "jwt_issuer" {
  type        = string
  description = "JWT token issuer name"
  default     = "FeatureFlow"
}

variable "jwt_audience" {
  type        = string
  description = "JWT token audience name"
  default     = "FeatureFlowClient"
}

# ============================================================================
# Container Apps Configuration
# ============================================================================

variable "api_cpu" {
  type        = string
  description = "CPU allocation for API container (e.g., 0.25, 0.5, 1.0)"
  default     = "0.25"
}

variable "api_memory" {
  type        = string
  description = "Memory allocation for API container (e.g., 0.5Gi, 1Gi)"
  default     = "0.5Gi"
}

variable "web_cpu" {
  type        = string
  description = "CPU allocation for Web container (e.g., 0.25, 0.5, 1.0)"
  default     = "0.25"
}

variable "web_memory" {
  type        = string
  description = "Memory allocation for Web container (e.g., 0.5Gi, 1Gi)"
  default     = "0.5Gi"
}

variable "min_replicas" {
  type        = number
  description = "Minimum number of container replicas (0 = scale to zero)"
  default     = 0
}

variable "max_replicas" {
  type        = number
  description = "Maximum number of container replicas"
  default     = 1
}

# ============================================================================
# Container Image Configuration
# ============================================================================

variable "api_image" {
  type        = string
  description = "Docker image for the API container (overridden by CI/CD)"
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "web_image" {
  type        = string
  description = "Docker image for the Web container (overridden by CI/CD)"
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

# ============================================================================
# Terraform State Backend Configuration
# Note: The Terraform backend is configured via `terraform init -backend-config=...`
# and cannot reference input variables. The values below are for documentation
# purposes only and are not used by the backend block in backend.tf.
# ============================================================================

variable "tfstate_resource_group" {
  type        = string
  description = "Resource group name containing the Terraform state storage account"
  default     = ""
}

variable "tfstate_storage_account" {
  type        = string
  description = "Storage account name for Terraform state"
  default     = "nexoflowtfstated89f8823"
}

variable "tfstate_container" {
  type        = string
  description = "Blob container name for Terraform state"
  default     = "tfstate"
}
