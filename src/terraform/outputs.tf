output "tenant_id" {
  value = data.azuread_client_config.current.tenant_id
}

output "subscription_id" {
  value = var.subscription_id
}

output "location" {
  value = azurerm_resource_group.default.location
}

output "apim_v1_gateway_url" {
  value = azurerm_api_management.v1.gateway_url
}

output "apim_v2_gateway_url" {
  value = azurerm_api_management.v2.gateway_url
}

output "openai_endpoint" {
  value = azapi_resource.ai_foundry.output.properties.endpoint
}

output "content_safety_endpoint" {
  value = azapi_resource.ai_foundry.output.properties.endpoint
  # value = azurerm_cognitive_account.content_safety.endpoint
}

output "application_insights_connection_string" {
  value     = azurerm_application_insights.default.connection_string
  sensitive = true
}
