(prop "long" "Set the 'long' property to change this.")

(prop "description" ^"(this:short)\n(this:long)\n(if
	(equal (length this.contents) 0)
	*("There doesn't appear to be anything here.")
	*("Also here (short_list_with_on this.contents)")
	)\n(if (equal (length (coalesce this.links ^())) 0)
		*("There are no obvious exits.")
		*("Obvious exits: (strcat $(map "link" this.links *(link.name))).")
	)"
)