resource "azurerm_api_management" "v1" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "apim-${var.codename}-v1-${random_id.default.hex}"
  publisher_name      = var.publisher_name
  publisher_email     = var.publisher_email
  sku_name            = "Developer_1"

  identity {
    type         = "SystemAssigned, UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.apim.id]
  }
}

resource "azurerm_api_management" "v2" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "apim-${var.codename}-v2-${random_id.default.hex}"
  publisher_name      = var.publisher_name
  publisher_email     = var.publisher_email
  sku_name            = "BasicV2_1"

  identity {
    type         = "SystemAssigned, UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.apim.id]
  }
}
