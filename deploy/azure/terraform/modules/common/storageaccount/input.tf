variable "tenant" {}
variable "environment" {}
variable "region" {}
variable "role" {}

variable "resourceGroupName" {}
variable "name" {} # resource name override

variable accountKind {
    type = string
	default = "StorageV2"

    validation {
        condition       = contains(["BlobStorage", "BlockBlobStorage", "FileStorage", "Storage", "StorageV2"], var.accountKind)
        error_message   = "accountTier is not valid for this cloud provider"
    }    
}
variable "accountTier" {
    type = string
	default = "Standard"

    validation {
        condition       = contains(["Standard", "Premium"], var.accountTier)
        error_message   = "accountTier is not valid for this cloud provider"
    }    
}
variable "accessTier" {
    type = string
	default = "Hot"

    validation {
        condition       = contains(["Hot", "Cool"], var.accessTier)
        error_message   = "accountTier is not valid for this cloud provider"
    }    
}

variable "replicationType" {
    type = string
    default = "GRS"

    validation {
        condition       = contains(["LRS","ZRS","GRS","RA-GRS"], var.replicationType)
        error_message   = "replicationType is not valid for this cloud provider"
    }
}

variable hierarchicalNameSpace {
    type = bool
    default = false
}