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
					*(clone match ^(into word))
				)
			)
		)
	)
)

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
					
(defun "extract_token_list" ^("token") ^() /* Returns a list of all the tokens in the linked list begun by 'token'. */
	*(mapex "token" token *(token) *(token.next))
)
				
(defun "flipper" ^("first" "middle" "last") ^() /* First match middle, then match last, then try to match first in the gap between start of input and middle */
	*(defun "" ^("matches") ^("first" "middle" "last")
		*(cat $(map "match" matches
			*(map "whole_match" 
				(cat $(where "first_match" 
					(first
						(map "last_match" 
							(cat $(map "token" (extract_token_list match.token) 
								*(last (middle ^((clone match ^("token" token) ^("flip_start" match.token) ^("middle_start" token)))))
							))
							*(clone last_match ^("token" last_match.flip_start) ^("end" last_match.token))
						)
					)
					*(equal first_match.token first_match.middle_start)
				))
				*(clone whole_match ^("token" whole_match.end))
			)
		))
	)
)
			
			