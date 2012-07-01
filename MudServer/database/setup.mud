(defun "or" ^("A" "B") ^() 
	*(defun "" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))))

(defun "keyword" ^("word") ^() 
	*(defun "" ^("matches") ^("word") 
		*(map "match"
			(where "match" matches *(equal word match.token.word))
			*(new_match match match.token.next))))

(defun "none" ^() ^() 
	*(defun "" ^("matches") ^() 
		*(where "match" matches *(equal null match.token))))

(defun "rest" ^("into") ^() 
	*(defun "" ^("matches") ^("into") 
		*(map "match" 
			(where "match" matches *(not (equal null match.token))) 
			*(lastarg (set match into (substr command match.token.place)) (set match "token" null) match))))

(defun "sequence" ^("matcher_list") ^()
	*(defun "" ^("matches") ^("matcher_list")
		*(lastarg
			(for "matcher" matcher_list *(var "matches" (matcher matches)))
			matches)))

(defun "optional" ^("matcher") ^()
	*(defun "" ^("matches") ^("matcher")
		*(cat (matcher matches) matches)))

(defun "anyof" ^("word_list") ^()
	*(defun "" ^("matches") ^("word_list")
		*(cat
			$(map "word" word_list *((keyword word) matches)))))

(defun "anything" ^() ^()
	*(defun "" ^("matches") ^() *(lastarg matches)))

(defun "contains" ^("list" "what") ^()
	*(atleast (count "item" list *(equal item what)) 1))

(defun "location_contents" ^("match") ^() /* Return the contents of the actor's location. Perhaps remove the actor? */
	*(contents actor.location))

(defun "object" ^("source" "into") ^() /* Match an object in the list returned by 'source' */
	*(defun "" ^("matches") ^("source" "into")
		*(cat 																						/* Combine list of lists into single list */
			$(map "match" matches																	/* Map existing matches to new matches */
				*(map "object" 																		/* Map objects to matches */
					(where "object" (source match) 													/* Result is a list of objects who'se noun property contains the next word in the command */
						*(contains (coalesce object.nouns ^()) match.token.word))
					*(new_match match match.token.next into object))))))

/*
	Flipper - Matches the first object relative the second
	Object - Matches an object
*/

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short)\n"))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(verb "say" (rest "text") (defun "" ^("match" "actor") ^() *(echo actor match.text)))

(verb "eval" (rest "code") (defun "" ^("match" "actor") ^() *(echo actor (eval match.code))))

(verb "look" (none) 
	(defun "" ^("match" "actor") ^() 
		*(nop
			(echo actor 
"You are in (actor.location.path).
(actor.location.long)
(if (equal (length (contents actor.location)) 0) 
	*("There doesn't appear to be anything here.")
	*("Some important objects: (short_list (contents actor.location))"))"))))

(verb "look" (sequence ^((optional (keyword "at")) (object location_contents "object")))
	(defun "" ^("match" "actor") ^()
		*(echo actor (coalesce match.object.short "You see nothing special."))))

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))

(verb "functions" (none)
	(defun "" ^("match" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n"))))


