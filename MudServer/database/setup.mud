(defun "or" ^("A" "B") ^() 
	*(defun "" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))))

(defun "keyword" ^("word") ^() 
	*(defun "" ^("matches") ^("word") 
		*(map "match"
			(where "match" matches *(equal word match.token.word))
			*(clone match ^("token" match.token.next)))))

(defun "none" ^() ^() 
	*(defun "" ^("matches") ^() 
		*(where "match" matches *(equal null match.token))))

(defun "rest" ^("into") ^() 
	*(defun "" ^("matches") ^("into") 
		*(map "match" 
			(where "match" matches *(not (equal null match.token))) 
			*(clone match ^("token" null) ^(into (substr command match.token.place))))))

(defun "sequence" ^("matcher_list") ^()
	*(defun "" ^("matches") ^("matcher_list")
		*(lastarg
			(for "matcher" matcher_list *(var "matches" (matcher matches)))
			matches)))

(defun "optional" ^("matcher") ^()
	*(defun "" ^("matches") ^("matcher")
		*(cat (matcher matches) matches)))

(defun "anyof" ^("word_list" "into") ^()
	*(defun "" ^("matches") ^("word_list" "into")
		*(cat
			$(map "word" word_list 
				*(map "match" ((keyword word) matches)
					*(clone match ^(into word)))))))

(defun "anything" ^() ^()
	*(defun "" ^("matches") ^() *(matches)))

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
					*(clone match ^("token" match.token.next) ^(into object)))))))

(defun "complete" ^("nested") ^()
	*(defun "" ^("matches") ^("nested")
		*(where "match" (nested matches) 
			*(equal match.token null))))
			
(defun "here" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((keyword "here") matches) 
			*(clone match ^(into actor.location)))))
			
(defun "me" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((keyword "me") matches)
			*(clone match ^(into actor)))))
					
/*
	Flipper - Matches the first object relative the second
	Object - Matches an object
*/

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short), "))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(defun "remove" ^("what" "list") ^() *(where "item" (coalesce list ^()) *(not (equal item what))))
(defun "add" ^("what" "list") ^() *(cat (coalesce list ^()) ^(what)))
(defun "prop_remove" ^("object" "property" "item") ^() *(set object property (remove item (object.(property)))))
(defun "prop_add" ^("object" "property" "item") ^() *(set object property (add item (coalesce (object.(property)) ^()))))

(defun "move_object" ^("what" "to") ^() 
	*(nop
		(if (not (equal what.location null)) 
			*(prop_remove what.location "contents" what))
		(prop_add to "contents" what)))

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


(verb "look" (complete (sequence ^((optional (keyword "at")) (object location_contents "object"))))
	(defun "" ^("match" "actor") ^()
		*(echo actor (coalesce match.object.short "You see nothing special."))))

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))

(verb "functions" (none)
	(defun "" ^("match" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name) - (function.shortHelp)\n"))))

(set system "on_unknown_verb" (defun "" ^("command" "actor") ^() 
	*(echo actor "I didn't understand your command '(command)'.")))
	
(verb "get" (object location_contents "object")
	(defun "" ^("match" "actor") ^()
		*(nop
			(move_object match.object actor)
			(echo actor "You take (coalesce match.object.a "a (match.object.short)")."))))
			
(verb "examine" (or (or (me "object") (here "object")) (object location_contents "object"))
	(defun "" ^("match" "actor") ^()
		*(echo actor
			(strcat
				$(map "prop_name" (members match.object)
					*("(prop_name): (match.object.(prop_name)), "))))))
