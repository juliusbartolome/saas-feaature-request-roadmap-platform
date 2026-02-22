# ============================================================================
# FeatureFlow - Terraform Outputs
# ============================================================================

# ============================================================================
# Resource Group
# ============================================================================

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "resource_group_id" {
  description = "ID of the resource group"
  value       = azurerm_resource_group.main.id
}

# ============================================================================
# Container Registry
# ============================================================================

output "acr_name" {
  description = "Name of the Azure Container Registry"
  value       = azurerm_container_registry.main.name
}

output "acr_login_server" {
  description = "Login server URL for the Azure Container Registry"
  value       = azurerm_container_registry.main.login_server
}

output "acr_id" {
  description = "ID of the Azure Container Registry"
  value       = azurerm_container_registry.main.id
}

# ============================================================================
# Container Apps
# ============================================================================

output "api_url" {
  description = "URL of the API Container App"
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}

output "web_url" {
  description = "URL of the Web Container App"
  value       = "https://${azurerm_container_app.web.ingress[0].fqdn}"
}

output "api_name" {
  description = "Name of the API Container App"
  value       = azurerm_container_app.api.name
}

output "web_name" {
  description = "Name of the Web Container App"
  value       = azurerm_container_app.web.name
}

output "container_apps_environment_id" {
  description = "ID of the Container Apps Environment"
  value       = azurerm_container_app_environment.main.id
}

# ============================================================================
# Key Vault
# ============================================================================

output "key_vault_name" {
  description = "Name of the Azure Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Azure Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

# ============================================================================
# Managed Identity
# ============================================================================

output "managed_identity_client_id" {
  description = "Client ID of the user-assigned managed identity"
  value       = azurerm_user_assigned_identity.container_apps.client_id
}

output "managed_identity_principal_id" {
  description = "Principal ID of the user-assigned managed identity"
  value       = azurerm_user_assigned_identity.container_apps.principal_id
}

# ============================================================================
# Useful Commands (for reference)
# ============================================================================

output "helpful_commands" {
  description = "Helpful Azure CLI commands for managing the deployment"
  value = {
    view_api_logs = "az containerapp logs show -n ${azurerm_container_app.api.name} -g ${azurerm_resource_group.main.name} --follow"
    view_web_logs = "az containerapp logs show -n ${azurerm_container_app.web.name} -g ${azurerm_resource_group.main.name} --follow"
    restart_api   = "az containerapp revision restart -n ${azurerm_container_app.api.name} -g ${azurerm_resource_group.main.name} --revision ${azurerm_container_app.api.name}--latest"
    restart_web   = "az containerapp revision restart -n ${azurerm_container_app.web.name} -g ${azurerm_resource_group.main.name} --revision ${azurerm_container_app.web.name}--latest"
    acr_login     = "az acr login --name ${azurerm_container_registry.main.name}"
  }
}
