variable "tenant_id" {
  default = ""
}

variable "subscription_id" {
  default = ""
}

variable "location" {
  default = "Japan East"
}

variable "codename" {
  default = ""
}

variable "publisher_name" {
  description = "API Management publisher name"
  type        = string
  default     = "Microsoft Taiwan"
}

variable "publisher_email" {
  description = "API Management publisher email"
  type        = string
  default     = "dino.wang@microsoft.com"
}
