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
			*(last (set match into (substr command match.token.place)) (set match "token" null) match))))

(defun "sequence" ^("matcher_list") ^()
	*(defun "" ^("matches") ^("matcher_list")
		*(last 
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
	*(defun "" ^("matches") ^() *(last matches)))

/*
	Flipper - Matches the first object relative the second
	Object - Matches an object
*/

(defun "short_list" ^("object_list") ^() 
	*(strcat $(map "object" object_list *("(object.short)\n"))))

(defun "contents" ^("mudobject") ^() *(coalesce mudobject.contents ^()))

(verb "test" (keyword "test") (defun "" ^("match" "actor") ^() *(echo actor "Success")))
(verb "test" (anyof ^("two" "one" "three")) (defun "" ^("match" "actor") ^() *(echo actor actor.location.long)))

(verb "say" (rest "text") (defun "" ^("match" "actor") ^() *(echo actor match.text)))

(verb "look" (none) 
	(defun "" ^("match" "actor") ^() 
		*(nop
			(echo actor 
"You are in (actor.location.path).
(actor.location.long)
(if (equal (length (contents actor.location)) 0) 
	*("There doesn't appear to be anything here.")
	*("Some important objects: (short_list (contents actor.location))"))"))))

(verb "look" (anything)
	(defun "" ^("match" "actor") ^()
		*(echo actor "I don't see that here.")))

(verb "functions" (none)
	(defun "" ^("match" "actor") ^()
		*(for "function" functions
			*(echo actor "(function.name)\n"))))

