(depend "grammar")
(depend "room")
(prop "long" "Set the 'long' property to change this.")
(prop "pronoun" "he")
(prop "on-get" (defun "" ^("actor") ^() *(echo actor "(this:short) wouldn't appreciate that.\n")))
(prop "prompt" ">")

(prop "description" *"(this:short)\n(this:long)\n(if
	(equal (length this.held) 0)
	*("")
	*("^(this:pronoun) is holding (short-list-no-isare this.held)\n")
	)"
)