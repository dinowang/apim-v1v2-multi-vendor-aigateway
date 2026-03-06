resource "azurerm_log_analytics_workspace" "default" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "log-${var.codename}-${random_id.default.hex}"
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "default" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "appi-${var.codename}-${random_id.default.hex}"
  workspace_id        = azurerm_log_analytics_workspace.default.id
  application_type    = "web"
}

resource "azurerm_api_management_logger" "v1" {
  resource_group_name = azurerm_resource_group.default.name
  api_management_name = azurerm_api_management.v1.name
  name                = "appi-logger"

  application_insights {
    instrumentation_key = azurerm_application_insights.default.instrumentation_key
  }
}

resource "azurerm_api_management_diagnostic" "v1" {
  resource_group_name      = azurerm_resource_group.default.name
  api_management_name      = azurerm_api_management.v1.name
  api_management_logger_id = azurerm_api_management_logger.v1.id
  identifier               = "applicationinsights"
}

resource "azurerm_api_management_logger" "v2" {
  resource_group_name = azurerm_resource_group.default.name
  api_management_name = azurerm_api_management.v2.name
  name                = "appi-logger"

  application_insights {
    instrumentation_key = azurerm_application_insights.default.instrumentation_key
  }
}

resource "azurerm_api_management_diagnostic" "v2" {
  resource_group_name      = azurerm_resource_group.default.name
  api_management_name      = azurerm_api_management.v2.name
  api_management_logger_id = azurerm_api_management_logger.v2.id
  identifier               = "applicationinsights"
}
