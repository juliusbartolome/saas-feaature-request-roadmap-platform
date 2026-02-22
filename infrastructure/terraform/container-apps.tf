# ============================================================================
# FeatureFlow - Container Apps
# ============================================================================

# ============================================================================
# API Container App
# ============================================================================

resource "azurerm_container_app" "api" {
  name                         = "ca-${var.project_name}-api-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  tags = local.common_tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  secret {
    name                = "postgres-connection-string"
    key_vault_secret_id = azurerm_key_vault_secret.postgres_connection_string.id
    identity            = azurerm_user_assigned_identity.container_apps.id
  }

  secret {
    name                = "jwt-secret"
    key_vault_secret_id = azurerm_key_vault_secret.jwt_secret.id
    identity            = azurerm_user_assigned_identity.container_apps.id
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "api"
      image  = var.api_image
      cpu    = var.api_cpu
      memory = var.api_memory

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment == "prod" ? "Production" : "Development"
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      }

      env {
        name        = "ConnectionStrings__Default"
        secret_name = "postgres-connection-string"
      }

      env {
        name        = "Jwt__Secret"
        secret_name = "jwt-secret"
      }

      env {
        name  = "Jwt__Issuer"
        value = var.jwt_issuer
      }

      env {
        name  = "Jwt__Audience"
        value = var.jwt_audience
      }

      liveness_probe {
        path                    = "/health"
        port                    = 8080
        transport               = "HTTP"
        interval_seconds        = 30
        timeout                 = 5
        failure_count_threshold = 3
      }

      readiness_probe {
        path                    = "/health"
        port                    = 8080
        transport               = "HTTP"
        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
      }
    }

    http_scale_rule {
      name                = "http-scaling"
      concurrent_requests = "100"
    }
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azurerm_role_assignment.kv_secrets_user
  ]
}

# ============================================================================
# Web Container App
# ============================================================================

resource "azurerm_container_app" "web" {
  name                         = "ca-${var.project_name}-web-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  tags = local.common_tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  ingress {
    external_enabled = true
    target_port      = 80
    transport        = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "web"
      image  = var.web_image
      cpu    = var.web_cpu
      memory = var.web_memory

      env {
        name  = "VITE_API_URL"
        value = "https://${azurerm_container_app.api.ingress[0].fqdn}/api/v1"
      }

      liveness_probe {
        path                    = "/"
        port                    = 80
        transport               = "HTTP"
        interval_seconds        = 30
        timeout                 = 5
        failure_count_threshold = 3
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.acr_pull,
    azurerm_container_app.api
  ]
}
