/* Matchers for building command parsers */

(defun "m-or" ^("A" "B") ^() 
	*(defun "" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))))

(defun "m-keyword" ^("word") ^() 
	*(lambda "lkeyword" ^("matches") ^("word") 
		*(map "match"
			(where "match" (where "match" matches *(not (equal null match.token))) *(equal word match.token.word))
			*(clone match ^("token" match.token.next)))))
			
(defun "m-nothing" ^() ^() 
	*(lambda "lnone" ^("matches") ^() 
		*(where "match" matches *(equal null match.token))))

(defun "m-rest" ^("into") ^() 
	*(lambda "lrest" ^("matches") ^("into") 
		*(map "match" 
			(where "match" matches *(not (equal null match.token))) 
			*(clone match ^("token" null) ^(into (substr match.command match.token.place)))
		)
	)
)

(defun "m-sequence" ^("matcher_list") ^()
	*(lambda "lsequence" ^("matches") ^("matcher_list")
		*(for "matcher" matcher_list *(var "matches" (matcher matches)))
	)
)

(defun "m-optional" ^("matcher") ^()
	*(lambda "loptional" ^("matches") ^("matcher")
		*(cat (matcher matches) matches)))

(defun "m-anyof" ^("word_list" "into") ^()
	*(lambda "lanyof" ^("matches") ^("word_list" "into")
		*(cat
			$(map "word" word_list 
				*(map "match" ((m-keyword word) matches)
					*(clone match ^(into word))
				)
			)
		)
	)
)

(defun "m-always-pass" ^() ^()
	*(lambda "lanything" ^("matches") ^() *(matches)))

(defun "m-complete" ^("nested") ^()
	*(lambda "lcomplete" ^("matches") ^("nested")
		*(where "match" (nested matches) 
			*(equal match.token null))))
			
(defun "m-here" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((m-keyword "here") matches) 
			*(clone match ^(into match.actor.location.object)))))
			
(defun "m-me" ^("into") ^()
	*(defun "" ^("matches") ^("into")
		*(map "match" ((m-keyword "me") matches)
			*(clone match ^(into match.actor)))))
			
(defun "m-any-visible-object" ^("into") ^() /*Should include 'my' to limit visibility*/
	*(m-if-exclusive (m-keyword "my")
		(m-if-exclusive (m-object (os-mine "actor") into)
			(m-nop)
			(m-fail "You don't seem to have that.\n")
		)
		(m-or (m-object (os-visible "actor") into) (m-or (m-here into) (m-me into)))
	)
)
					
(defun "m-flipper-rest" ^() ^() 
	*(defun "" ^("matches") ^() 
		*(map "match" 
			(where "match" matches *(not (equal match.middle_start match.token))) 
			*(clone match ^("token" match.middle_start))
		)
	)
)

(defun "m-flipper-nothing" ^() ^()
	*(lambda "lflipper_none" ^("matches") ^()
		*(where "match" matches *(equal match.token match.middle_start))
	)
)

(defun "m-flipper-complete" ^("matcher") ^()
	*(m-sequence ^(matcher (m-flipper-nothing)))
)
				
(defun "m-flipper" ^("first" "middle" "last") ^()
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
	"Searches input for all places where middle successfully matches, then attempts to match last, and finally matches first in the gap before where middle matched. If the middle matches, fail-matches from the first or last part will be returned."
)

(defun "m-set" ^("name" "value") ^()
	*(lambda "lset_val" ^("matches") ^("name" "value")
		*(map "match" matches *(clone match ^(name value)))
	)
)

(defun "m-set-l" ^("pairs") ^()
	*(lambda "lset_vals" ^("matches") ^("pairs")
		*(map "match" matches *(clone match $(pairs)))
	)
)

(defun "m-set-object-here" ^() ^()
	*(lambda "" ^("matches") ^()
		*(map "match" matches *(clone match ^("object" match.actor.location.object)))
	)
)

(defun "m-if" ^("matcher" "then" "else") ^()
	*(lambda "lif_matches" ^("matches") ^("matcher" "then" "else")
		*(cat (then (matcher matches)) (else matches))
	)
	"If $1 matches, call $2 with the results; call $3 with the original matches. Return both sets cated."
)

/* If it matches, it does not return matches that don't match. */
(defun "m-if-exclusive" ^("matcher" "then" "else") ^()
	*(lambda "lif_matches_exclusive" ^("matches") ^("matcher" "then" "else")
		*(let ^(^("results" (matcher matches)))
			*(if (greaterthan (length results) 0)
				*(then results)
				*(else matches)
			)
		)
	)
	"Same as m-if, except else branch is never explored if $1 matches anything."
)

(defun "m-fork" ^("A" "B") ^()
	*(lambda "lfork" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))
	)
	"Fork match set into two sets."
)

(defun "m-nop" ^() ^() *(lambda "lmatch_nop" ^("matches") ^() *(matches)) "Return matches unchanged.")

(defun "m-filter-failures" ^("matcher") ^()
	*(lambda "lfilter_failures" ^("matches") ^("matcher") 
		*(let ^(^("matches" (matcher matches)))
			*(let ^(
				^("pass" (where "match" matches *(equal match.fail null)))
				)
				*(if (greaterthan (length pass) 0) 
					*(pass)
					*(where "match" matches *(notequal match.fail null))
				)
			)
		)
	)
	"If there are 1 or more passing matches, return them. Otherwise, return failing matches."
)

(defun "m-preposition" ^() ^()
	*(m-anyof ^("in" "on" "under") "preposition")
)

(defun "m-?-preposition" ^() ^()
	*(m-if-exclusive (m-anyof ^("in" "on" "under") "preposition") 
		(m-nop) 
		(m-fork
			(m-set "preposition" "on")
			(m-set "preposition" "in")
		)
	)
)

(defun "m-?-all" ^() ^()
	*(m-if-exclusive (m-keyword "all")
		(m-set "all" true)
		(m-nop)
	)
)

(defun "m-from|preposition" ^() ^()
	*(m-if-exclusive (m-keyword "from")
		(m-?-preposition)
		(m-preposition)
	)
)

(defun "m-allows-preposition" ^("object") ^()
	*(lambda "lm-allows-preposition" ^("matches") ^("object")
		*(where "match" matches *(match.(object):("allow-(match.preposition)")))
	)
)

(defun "m-supporter" ^("message") ^()
	*(m-if-exclusive (m-any-visible-object "supporter")
		(m-if-exclusive (m-allows-preposition "supporter")
			(m-nop)
			(m-fail message)
		)
		(m-fail "I don't see that here.\n")
	)
)

(defun "m-relative-object" ^() ^()
	*(m-object (os-contents-v "supporter" "preposition") "object")
)

(defun "m-expand-supported-objects" ^() ^()
	*(m-expand-objects (os-contents-v "supporter" "preposition"))
)

(defun "m-expand-held-objects" ^() ^()
	*(m-expand-objects (os-contents "actor" "held"))
)

(defun "m-expand-objects" ^("source") ^()
	*(lambda "lm-expand-objects" ^("matches") ^("source")
		*(cat
			$(map "match" matches
				*(cat
					$(map "object" (source match) *(clone match ^("object" object)))
				)
			)
		)
	)
)

(defun "m-fail" ^("message") ^()
	*(lambda "lm-fail" ^("matches") ^("message")
		*(map "match" matches 
			*(if (equal match.fail null)
				*(clone match ^("fail" message))
				*(clone match)
			)
		)
	)
)

(defun "m-switch" ^("list" "tail") ^()
	*(if (greaterthan (length list) 1)
		*(m-if-exclusive (first (first list))
			(last (first list))
			(m-switch (sub-list list 1) tail)
		)
		*(m-if-exclusive (first (first list))
			(last (first list))
			tail
		)
	)
	"m-switch list tail: Implements a chain of m-if-exclusive. Each item in the list is the else clause of the item before it. Tail is the final else."
)

			
			