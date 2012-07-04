/* Matchers for building command parsers */

(defun "or" ^("A" "B") ^() 
	*(defun "" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))))

(defun "keyword" ^("word") ^() 
	*(defun "" ^("matches") ^("word") 
		*(map "match"
			(where "match" (where "match" matches *(not (equal null match.token))) *(equal word match.token.word))
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

(defun "complete" ^("nested") ^()
	*(defun "" ^("matches") ^("nested")
		*(where "match" (nested matches) 
			*(equal match.token null))))
			
(defun "here" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((keyword "here") matches) 
			*(clone match ^(into actor.location.object)))))
			
(defun "me" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((keyword "me") matches)
			*(clone match ^(into actor)))))
			
(defun "any_object" ^("into") ^()
	*(or (object (location_source "actor" "contents") into) (or (here into) (me into))))
					