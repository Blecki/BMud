/* Matchers for building command parsers */

(defun "or" ^("A" "B") ^() 
	*(defun "" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))))

(defun "keyword" ^("word") ^() 
	*(lambda "lkeyword" ^("matches") ^("word") 
		*(map "match"
			(where "match" (where "match" matches *(not (equal null match.token))) *(equal word match.token.word))
			*(clone match ^("token" match.token.next)))))
			
(defun "none" ^() ^() 
	*(lambda "lnone" ^("matches") ^() 
		*(where "match" matches *(equal null match.token))))

(defun "rest" ^("into") ^() 
	*(lambda "lrest" ^("matches") ^("into") 
		*(map "match" 
			(where "match" matches *(not (equal null match.token))) 
			*(clone match ^("token" null) ^(into (substr match.command match.token.place)))
		)
	)
)

(defun "sequence" ^("matcher_list") ^()
	*(lambda "lsequence" ^("matches") ^("matcher_list")
		*(for "matcher" matcher_list *(var "matches" (matcher matches)))
	)
)

(defun "optional" ^("matcher") ^()
	*(lambda "loptional" ^("matches") ^("matcher")
		*(cat (matcher matches) matches)))

(defun "anyof" ^("word_list" "into") ^()
	*(lambda "lanyof" ^("matches") ^("word_list" "into")
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
	*(lambda "lanything" ^("matches") ^() *(matches)))

(defun "complete" ^("nested") ^()
	*(lambda "lcomplete" ^("matches") ^("nested")
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
	*(or (object (visible_objects "actor") into) (or (here into) (me into))))
					
(defun "extract_token_list" ^("token") ^() /* Returns a list of all the tokens in the linked list begun by 'token'. */
	*(mapex "token" token *(token) *(token.next))
)

(defun "flipper_rest" ^() ^() 
	*(defun "" ^("matches") ^() 
		*(map "match" 
			(where "match" matches *(not (equal match.middle_start match.token))) 
			*(clone match ^("token" match.middle_start))
		)
	)
)

(defun "flipper_none" ^() ^()
	*(lambda "lflipper_none" ^("matches") ^()
		*(where "match" matches *(equal match.token match.middle_start))
	)
)

(defun "flipper_complete" ^("matcher") ^()
	*(sequence ^(matcher (flipper_none)))
)
				
(defun "flipper" ^("first" "middle" "last") ^() /* First match middle, then match last, then try to match first in the gap between start of input and middle */
	*(lambda "lflipper" ^("matches") ^("first" "middle" "last")
		*(cat $(map "match" matches
			*(let ^(
				^("middle_matches" 
					(cat $(map "token" (mapex "token" match.token *(token) *(token.next))
						*(middle ^((clone match ^("token" token) ^("flip_start" match.token) ^("middle_start" token))))
					))
				))
				*(let ^(^("last_matches" (last middle_matches)))
					*(let ^(
							^("fail_last" (where "match" last_matches *(notequal match.fail null)))
							^("pass_last" (where "match" last_matches *(equal match.fail null)))
						)
						*(if (greaterthan (length pass_last) 0) 
							*(map "whole_match" (cat 
								$(where "first_match"
									(first (map "match" pass_last *(clone match ^("token" match.flip_start) ^("end" match.token))))
									*(equal first_match.token first_match.middle_start)
								))
								*(clone whole_match ^("token" whole_match.end))
							)								
							*(fail_last)
						)
					)
				)
			)
		))
	)
)

		/* Rewrite flipper to return fail-match immediately if last part fails. - /if the middle matches anywhere treat it as a successful match!/ */
/*		*(cat $(map "match" matches
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
)*/
			
			