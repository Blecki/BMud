(prop "long" "Set the 'long' property to change this.")

(prop "description" 
*"(this:short)
(this:long)
(let ^(^("contents" (where "object" this.contents (notequal actor object))))
	(if	(equal (length contents) 0)
		("There doesn't appear to be anything here.")
		("Also here (actor.formatter.list-objects contents true true).")
	)
)
(if (equal (length (coalesce this.links ^())) 0)
		*("There are no obvious exits.")
		*("Obvious exits: (actor.formatter.list-links this.links)")
)"
)

(defun "add-detail" ^("to" "name" "text")
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-optional (m-keyword "at")) (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor")
			*(echo actor "[Looking at the (name)]\n(text)\n")
		)
		"Detail"
	)
)

(defun "add-adjective-detail" ^("to" "name" "adjectives" "text")
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-optional (m-keyword "at")) (m-?-adjectives adjectives) (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor")
			*(echo actor "[Looking at the (name)]\n(text)\n")
		)
		"Detail"
	)
)

(defun "add-keyword-detail" ^("to" "keywords" "name" "text")
	*(add-verb to "look"
		(m-complete (m-sequence ^((m-anyof keywords "keyword") (m-keyword name))))
		(lambda "ldetail" ^("matches" "actor")
			*(echo actor "[Looking ((first matches).keyword) the (name)]\n(text)\n")
		)
		"Keyword detail"
	)
)