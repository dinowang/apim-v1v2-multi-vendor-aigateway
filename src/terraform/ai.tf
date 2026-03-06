resource "azurerm_storage_account" "foundry" {
  location                 = azurerm_resource_group.default.location
  resource_group_name      = azurerm_resource_group.default.name
  name                     = "storfoundry${var.codename}${random_id.default.hex}"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_key_vault" "foundry" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "kv-foundry-${var.codename}-${random_id.default.hex}"
  tenant_id           = var.tenant_id
  sku_name            = "standard"
}

resource "azurerm_ai_services" "default" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "mf-${var.codename}-${random_id.default.hex}"
  sku_name            = "S0"
}

resource "azurerm_ai_foundry" "default" {
  location            = azurerm_ai_services.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "project-${var.codename}-${random_id.default.hex}"
  storage_account_id  = azurerm_storage_account.foundry.id
  key_vault_id        = azurerm_key_vault.foundry.id

  identity {
    type = "SystemAssigned"
  }
}

resource "azapi_resource" "ai_foundry" {
  location                  = azurerm_resource_group.default.location
  parent_id                 = azurerm_resource_group.default.id
  type                      = "Microsoft.CognitiveServices/accounts@2025-06-01"
  name                      = "aif-${var.codename}-${random_id.default.hex}"
  schema_validation_enabled = false

  body = {
    kind = "AIServices"
    sku = {
      name = "S0"
    }
    identity = {
      type = "SystemAssigned"
    }
    properties = {
      disableLocalAuth       = false
      allowProjectManagement = true
      customSubDomainName    = "aif-${var.codename}-${random_id.default.hex}"
    }
  }

  response_export_values = ["properties.endpoint"]
}

resource "azapi_resource" "gpt4o_deployment" {
  parent_id = azapi_resource.ai_foundry.id
  type      = "Microsoft.CognitiveServices/accounts/deployments@2023-05-01"
  name      = "gpt-4o"

  body = {
    sku = {
      name     = "GlobalStandard"
      capacity = 10
    }
    properties = {
      model = {
        format  = "OpenAI"
        name    = "gpt-4o"
        version = "2024-11-20"
      }
    }
  }
}

# resource "azurerm_cognitive_account" "content_safety" {
#   location            = azurerm_resource_group.default.location
#   resource_group_name = azurerm_resource_group.default.name
#   name                = "cs-${var.codename}-${random_id.default.hex}"
#   kind                = "ContentSafety"
#   sku_name            = "S0"
# }
