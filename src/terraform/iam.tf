resource "azurerm_user_assigned_identity" "apim" {
  location            = azurerm_resource_group.default.location
  resource_group_name = azurerm_resource_group.default.name
  name                = "id-${var.codename}-apim-${random_id.default.hex}"
}

# UAMI → Microsoft Foundry (Cognitive Services OpenAI User)
resource "azurerm_role_assignment" "apim_openai" {
  scope                = azapi_resource.ai_foundry.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = azurerm_user_assigned_identity.apim.principal_id
}


# UAMI → Content Safety (Cognitive Services User)
resource "azurerm_role_assignment" "apim_content_safety" {
  scope                = azapi_resource.ai_foundry.id
  role_definition_name = "Cognitive Services User"
  principal_id         = azurerm_user_assigned_identity.apim.principal_id
}

# UAMI → Content Safety (Cognitive Services User)
# resource "azurerm_role_assignment" "apim_content_safety" {
#   scope                = azurerm_cognitive_account.content_safety.id
#   role_definition_name = "Cognitive Services User"
#   principal_id         = azurerm_user_assigned_identity.apim.principal_id
# }
