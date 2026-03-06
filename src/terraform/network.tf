# resource "azurerm_network_security_group" "default" {
#   location            = azurerm_resource_group.default.location
#   resource_group_name = azurerm_resource_group.default.name
#   name                = "nsg-${var.codename}-default"
# }

# resource "azurerm_virtual_network" "default" {
#   location            = azurerm_resource_group.default.location
#   resource_group_name = azurerm_resource_group.default.name
#   name                = "vnet-${var.codename}-${random_id.default.hex}"
#   address_space       = ["10.0.0.0/16"]

#   subnet {
#     name             = "GatewaySubnet"
#     address_prefixes = [ "10.0.1.0/24" ]
#   }

#   subnet {
#     name             = "ApimSubnet"
#     address_prefixes = [ "10.0.2.0/24" ]
#     security_group   = azurerm_network_security_group.default.id
#   }

#   subnet {
#     name             = "AppGatewaySubnet"
#     address_prefixes = [ "10.0.3.0/24" ]
#     security_group   = azurerm_network_security_group.default.id
#   }

#   subnet {
#     name             = "DefaultSubnet"
#     address_prefixes = [ "10.0.4.0/24" ]
#     security_group   = azurerm_network_security_group.default.id
#   }

#   subnet {
#     name             = "VnetIntegrationSubnet"
#     address_prefixes = [ "10.0.5.0/24" ]
#     security_group   = azurerm_network_security_group.default.id
#   }

#   subnet {
#     name             = "DatabaseSubnet"
#     address_prefixes = [ "10.0.6.0/24" ]
#     security_group   = azurerm_network_security_group.default.id
#   }
# }
