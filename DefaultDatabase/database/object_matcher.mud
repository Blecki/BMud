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

(defun "get-all-visible-objects" [actor] []
	(cat
		(where "object" (coalesce actor.location.object.contents ^()) *(notequal object actor))
		(coalesce actor.held ^())
		(coalesce actor.worn ^())
		$(map "object" (coalesce actor.location.object.contents ^())
			(cat (coalesce object.on ^()) (coalesce object.in ^())))
		$(map "object" (cat (coalesce actor.held ^()) (coalesce actor.worn ^()))
			(cat (coalesce object.on ^()) (coalesce object.in ^())))
	)
)

(defun "os-visible" ^("relative") ^() /*Everything that 'relative' can see*/
	(lambda "los-visible" [match] [relative] (get-all-visible-objects match.(relative)))
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

(defun "m-object" ^("source" "into") ^()
	*(lambda "lm-object" ^("matches") ^("source" "into")
		*(reverse (sort "match" 
			(where "match" 
				(cat 
					$(map "match" matches		
						*(map "object" (source match)
							*(let ^(^("nouns" (coalesce object.nouns ^())) ^("adjectives" (coalesce object.adjectives ^())))
								*(lastarg
									(while 
										*(and (notequal match.token null) (contains adjectives match.token.word)) 
										*(var "match" (clone match 
											^("token" match.token.next) 
											^("adjectives_matched" (add (coalesce match.adjectives_matched 0) 1))
										))
									)
									(if (and (notequal match.token null) (contains nouns match.token.word))
										*(clone match ^("token" match.token.next) ^(into object))
										*(clone match ^("fail" "Did not match object."))
									)
								)
							)
						)
					)
				)
				*(equal match.fail null)
			)
			*(coalesce match.adjectives_matched 0)
		))
	)
)	
