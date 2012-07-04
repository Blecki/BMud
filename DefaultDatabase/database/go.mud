(depend "move_object")
(discard_verb "go")

(defun "open_link" ^("object" "name" "to") ^() 
	*(prop_add object "links" (record ^("name" name) ^("to" to)))
)

(defun "link_matcher" ^() ^() 
	*(defun "" ^("matches") ^()
		*(map "match" 
			(where "match" matches  
				*(atleast (count "link" actor.location.object.links *(equal link.name match.token.word)) 1)
			)
			*(clone match ^("token" match.token.next) ^("link" match.token.word))
		)
	)
)



(verb "go" (link_matcher)
	(defun "" ^("match" "actor") ^()
		*(nop
			(echo actor.location.object.contents "(actor:short) went (match.link).")
			(move_object actor (first (where "link" actor.location.object.links *(equal match.link link.name))) "contents")
			(echo actor.location.object.contents "(actor:short) arrives.")
		)
	)
)
			