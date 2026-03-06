terraform {
  required_providers {
    azapi = {
      source = "azure/azapi"
    }
  }

  backend "local" {
    path = "terraform.tfstate"
  }
}

provider "azurerm" {
  tenant_id       = var.tenant_id
  subscription_id = var.subscription_id
  features {}
}

provider "azapi" {
  tenant_id       = var.tenant_id
  subscription_id = var.subscription_id
}

provider "azuread" {}
provider "random" {}

resource "random_id" "default" {
  byte_length = 3
}

data "azuread_client_config" "current" {}

resource "azurerm_resource_group" "default" {
  location = var.location
  name     = "rg-${var.codename}-${random_id.default.hex}"
}
