# ============================================================================
# FeatureFlow - Terraform Backend Configuration
# ============================================================================
# 
# This file configures the Azure Blob Storage backend for Terraform state.
# The storage account must exist before running terraform init.
#
# Prerequisites:
# 1. Storage account 'nexoflowtfstated89f8823' must exist
# 2. A blob container named 'tfstate' must be created in the storage account
#
# To create the container:
#   az storage container create --name tfstate \
#     --account-name nexoflowtfstated89f8823
#
# To initialize Terraform with this backend:
#   terraform init \
#     -backend-config="resource_group_name=<your-storage-account-rg>" \
#     -backend-config="subscription_id=<your-subscription-id>"
#
# ============================================================================

terraform {
  backend "azurerm" {
    storage_account_name = "nexoflowtfstated89f8823"
    container_name       = "tfstate"
    key                  = "featureflow.tfstate"
    use_azuread_auth     = true
  }
}
