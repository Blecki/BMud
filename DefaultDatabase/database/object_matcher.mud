/* Object sources for object matcher */

(defun "os-contents" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative" "list") 
		*(coalesce match.(relative).(list) ^())
	)
)

(defun "os-contents-v" ^("relative" "list") ^()
	*(defun "" ^("match") ^("relative" "list")
		*(coalesce match.(relative).(match.(list)) ^())
	)
)

(defun "os-contents-l" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(lambda "lcontents_source_list" ^("match") ^("relative" "list") 
		*(cat $(map "item" list *(coalesce match.(relative).(item) ^())))
	)
)

(defun "os-location" ^("relative") ^() /*Search relative some object for the items*/
	*(defun "" ^("match") ^("relative") 
		*(cat 
			(where "object" (coalesce match.(relative).location.object.contents ^()) *(notequal object match.(relative)))
			$(map "object" (where "object" (coalesce match.(relative).location.object.contents ^()) *(notequal object match.(relative)))
				*(cat (coalesce object.on ^()) (coalesce object.in ^()))
			)
		)
	)
)

(defun "os-visible" ^("relative") ^() /*Everything that 'relative' can see*/
	*(defun "" ^("match") ^("relative")
		*(cat
			(where "object" (coalesce match.(relative).location.object.contents ^()) *(notequal object match.(relative)))
			(coalesce match.(relative).held ^())
			(coalesce match.(relative).worn ^())
			$(map "object" (coalesce match.(relative).location.object.contents ^())
				*(cat (coalesce object.on ^()) (coalesce object.in ^())))
			$(map "object" (cat (coalesce match.(relative).held ^()) (coalesce match.(relative).worn ^()))
				*(cat (coalesce object.on ^()) (coalesce object.in ^())))
		)
	)
)

(defun "os-mine" ^("relative") ^() /*Everything relative can see and also has*/
	*(defun "" ^("match") ^("relative")
		*(cat
			(coalesce match.(relative).held ^())
			(coalesce match.(relative).worn ^())
			$(map "object" (cat (coalesce match.(relative).held ^()) (coalesce match.(relative).worn ^()))
				*(cat (coalesce object.on ^()) (coalesce object.in ^())))
		)
	)
)

(defun "os-allow-preposition" ^("source") ^()
	*(lambda "lallow_preposition" ^("match") ^("source")
		*(where "object" (source match) *(object:("allow_(match.preposition)")))
	)
)

(defun "os-cat" ^("A" "B") ^() /* Cat two sources together*/
	*(defun "" ^("match") ^("A" "B") 
		*(cat (A match) (B match))
	)
)

(defun "m-object" ^("source" "into") ^() /* Match an object in the list returned by 'source' */
	*(lambda "lobject" ^("matches") ^("source" "into")
		*(cat 																						/* Combine list of lists into single list */
			$(map "match" matches																	/* Map existing matches to new matches */
				*(map "object" 																		/* Map objects to matches */
					(where "object" (source match) 													/* Result is a list of objects who'se noun property contains the next word in the command */
						*(contains (coalesce object.nouns ^()) match.token.word))
					*(clone match ^("token" match.token.next) ^(into object)))))))
