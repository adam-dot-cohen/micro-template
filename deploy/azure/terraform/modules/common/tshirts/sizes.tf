variable "sizeMap" {
	type = map(
				object({
					abbrev = string
					name = string
				})
			)
	default = {
		"xs" = { name = "x-small"}
		"s" = {  name = "small"}
		"m" = {  name = "medium"}
		"l" = {  name = "large"}
		"xl" = {  name = "x-large"}
		"xxl" = { name = "xx-large"}
	}
}