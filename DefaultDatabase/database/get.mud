(depend "move_object")
(discard_verb "get")
(alias "take" "get")


(prop "take" (defun "" ^("actor" "object" "message_suffix") ^()
	*(if (equal object.on_get null) /* Invoke default get behavior. */
		*(
			(if (object.can_get actor)
				*(nop
					(move_object object actor "held")
					(echo actor "You take (object:a)(message_suffix).\n")
					(echo 
						(where "player" actor.location.object.contents *(notequal player actor)) 
						"(actor:short) takes (object:a)(message_suffix).\n"
					)	
				)
				*(echo actor "You can't get that.\n")
			)
		)
		*(object.on_get actor) /* invoke custom get behavior */
	)
))

(prop "take_from" (defun "" ^("actor" "object" "preposition" "from") ^()
	*((load "get").take actor object " from (preposition) (from:the)")
))



(defun "contents_source_rel_list" ^("relative" "list") ^()
	*(defun "" ^("match") ^("relative" "list")
		*(cat
			$(map "l" (match.(list))
				*(coalesce match.(relative).(l) ^())
			)
		)
	)
)

/* Applies setting to every match. For use with optional_mod */
(defun "set_val" ^("name" "value") ^()
	*(lambda "lset_val" ^("matches") ^("name" "value")
		*(map "match" matches *(clone match ^(name value)))
	)
)

(defun "set_vals" ^("pairs") ^()
	*(lambda "lset_vals" ^("matches") ^("pairs")
		*(map "match" matches *(clone match $(pairs)))
	)
)

/* If matcher matches, then will be called with the match. If not, else is called. */
(defun "if_matches" ^("matcher" "then" "else") ^()
	*(lambda "lif_matches" ^("matches") ^("matcher" "then" "else")
		*(cat (then (matcher matches)) (else matches))
	)
)

(defun "contents_source_list" ^("relative" "list") ^() /*Search relative some object for the items*/
	*(lambda "lcontents_source_list" ^("match") ^("relative" "list") 
		*(cat $(map "item" list *(coalesce match.(relative).(item) ^())))
	)
)

/* If it matches, it does not return matches that don't match. */
(defun "if_matches_exclusive" ^("matcher" "then" "else") ^()
	*(lambda "lif_matches_exclusive" ^("matches") ^("matcher" "then" "else")
		*(let ^(^("results" (matcher matches)))
			*(if (greaterthan (length results) 0)
				*(then results)
				*(else matches)
			)
		)
	)
)

(defun "fork" ^("A" "B") ^()
	*(lambda "lfork" ^("matches") ^("A" "B") 
		*(cat (A matches) (B matches))
	)
)

(defun "match_nop" ^() ^() *(lambda "lmatch_nop" ^("matches") ^() *(matches)))

(defun "sort_and_discard" ^("property" "value" "matcher") ^()
	*(lambda "lsort_and_discard" ^("matches") ^("property" "value" "matcher") 
		*(let ^(^("matches" (matcher matches)))
			*(let ^(
				^("pass" (where "match" matches *(equal match.(property) value)))
				)
				*(if (greaterthan (length pass) 0) 
					*(pass)
					*(where "match" matches *(notequal match.(property) value))
				)
			)
		)
	)
)

(defun "filter_failures" ^("matcher") ^()
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
)

(defun "collapse" ^("matcher") ^()	
	*(lambda "lcollapse" ^("matches") ^("matcher") 
		*(let ^(^("apply_to" (first matches)))
			*(if (notequal apply_to null)
				*(let ^(^("post_matcher" (first (matcher ^(apply_to)))))
					*(if (notequal post_matcher null)
						*(^(post_matcher))
						*(^())
					
					)
				)
				*(^())
			)
		)
	)
)

(defun "preposition" ^() ^()
	*(anyof ^("in" "on" "under") "preposition")
)

(defun "expand_objects" ^() ^()
	*(lambda "lexpand_objects" ^("matches") ^()
		*(cat 
			$(map "match" matches 
				*(cat
					$(map "object" (coalesce match.supporter.(match.preposition) ^())
						*(clone match ^("object" object))
					)
				)
			)
		)
	)
)

(defun "optional_preposition" ^() ^()
	*(if_matches_exclusive (anyof ^("in" "on" "under") "preposition") 
		(match_nop) 
		(fork 
			(set_val "preposition" "on")
			(set_val "preposition" "in")
		)
	)
)

(defun "optional_all" ^() ^()
	*(if_matches_exclusive (keyword "all")
		(set_val "all" true)
		(match_nop)
	)
)

(defun "from_or_preposition" ^() ^()
	*(if_matches_exclusive (keyword "from")
		(optional_preposition)
		(preposition)
	)
)

(defun "supporter" ^() ^()
	*(if_matches_exclusive (keyword "my") 
		(if_matches_exclusive (object (allow_preposition (contents_source_list "actor" ^("held" "worn"))) "supporter")
			(match_nop)
			(sequence ^((rest "supporter_name") (set_fail "You don't seem to have that.\n")))
		)
		(if_matches_exclusive (object (allow_preposition (visible_objects "actor")) "supporter")
			(match_nop)
			(sequence ^((rest "supporter_name") (set_fail "I don't see that here. [A]\n")))
		)
	)
)

(defun "rel_object" ^() ^()
	*(object (contents_source_rel "supporter" "preposition") "object")
)

(defun "set_fail" ^("message") ^()
	*(lambda "lset_fail" ^("matches") ^("message")
		*(map "match" matches 
			*(if (equal match.fail null)
				*(clone match ^("fail" message))
				*(clone match)
			)
		)
	)
)
					
/* GET [ALL] X ((FROM [IN/ON/UNDER]) | IN/ON/UNDER) [MY] Y */
(verb "get"
	(filter_failures
		(if_matches_exclusive (none) (set_fail "Get what?\n")
			(if_matches_exclusive (sequence ^((keyword "all") (none))) (set_fail "Get all what?\n") /*This should expand all visible objects*/
				(if_matches_exclusive (complete (sequence ^((optional_all) (object (location_source "actor") "object")))) 
				/* Should set appropriate from/prep/supporter values on match */
					(match_nop)
					(if_matches_exclusive
						(flipper
							(if_matches_exclusive (keyword "all") 
								(sequence ^((set_val "all" true) 
									(if_matches_exclusive (flipper_none)
										(expand_objects)
										(if_matches_exclusive (flipper_complete (rel_object))
											(match_nop)
											(sequence ^((flipper_rest) (set_fail ^"I can't find that (this.preposition) (this.supporter:the).\n")))
										)
									)
								))
								(if_matches_exclusive (flipper_none)
									(set_fail ^"Get what from (this.preposition) (this.supporter:the)?\n")
									(if_matches_exclusive (flipper_complete (rel_object))
										(match_nop)
										(sequence ^((flipper_rest) (set_fail ^"I can't find that (this.preposition) (this.supporter:the).\n")))
									)
								)								
							)
							(from_or_preposition)
							(if_matches_exclusive (none)
								(set_fail "Get from what?\n")
								(complete (supporter))
							)
						)
						(match_nop)
						(set_fail "I don't see that here. [B]")
					)
				)
			)
		)
	)
	(defun "" ^("matches" "actor") ^()
		*(if (notequal (first matches).fail null)
			*(echo actor (first matches):fail)
			*(if (equal (first matches).all true)
				*(for "match" matches
					*(imple_get actor match)
				)
				*(nop
					*(if (greaterthan (length matches) 1) *(echo actor "[Multiple possible matches. Accepting first match.]\n"))
					(imple_get actor (first matches))
				)
			)						
		)
	)
	"GET [ALL] X \(\(FROM [IN/ON/UNDER]\) | IN/ON/UNDER\) [MY] Y"
)

(defun "imple_get" ^("actor" "match") ^()
	*(if (notequal match.object.location.object actor.location.object)
		*(nop
			(echo actor "[Taking (match.object:a) from (match.object.location.list) (match.object.location.object:the).]\n")
			((load "get").take actor match.object " from (match.object.location.list) (match.object.location.object:the)")
		)
		*(nop
			(echo actor "[Taking (match.object:a).]\n")
			((load "get").take actor match.object "")
		)
	)
)
