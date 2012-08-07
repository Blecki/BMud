(defun "say" [actor text]
	(nop
		(echo actor "You say \"(text)\".\n")
		(echo (where "object" actor.location.object.contents (notequal actor object)) "^(actor:short) says \"(text)\".\n")
	)
)

(defun "emote" [actor metext themtext]
	(nop
		(echo actor "(metext)\n")
		(echo (where "object" actor.location.object.contents (notequal actor object)) "(themtext)\n")
	)
)

(add-global-verb "say" (m-rest "text") (lambda "" ^("matches" "actor") (say actor (first matches).text)) "run your mouth")
(add-global-alias "'" "say")

(add-global-verb "emote" (m-rest "text") (lambda "" ^("matches" "actor") (emote actor "You ((first matches).text)." "^(actor:short) ((first matches).text).")) "Express yourself.")
(add-global-alias "\"" "emote")

(add-global-verb "dance" (m-nothing) (lambda "" [matches actor] (emote actor "You dance around elegantly." "^(actor:short) fumbles about like an eppileptic.")) "Fail to express yourself.")

