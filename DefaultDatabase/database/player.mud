(depend "grammar")
(depend "room")
(prop "long" "Set the 'long' property to change this.")
(prop "pronoun" "he")

(prop "description" ^"(this:short)\n(this:long)\n(if
	(equal (length this.held) 0)
	*("")
	*("(this:pronoun) is holding (short_list this.held)\n")
	)"
)