(defun "send-to-channel" [channel actor text] []
	(echo (where "player" players (contains player.channels channel)) "^(channel): <(actor:short)> (text)\n")
)		

(defun "add-channel" [name can-subscribe description] []
	(nop
		(let ^(^("chat" (load "chat")))
			(prop-add chat "channels" (record ^("name" name) ^("can-subscribe" can-subscribe) ^("description" description)))
		)
		(add-global-verb name (m-if-exclusive (m-rest "text") (m-nop) (m-fail "And what message were you going to send to that channel?"))
			(lambda "lchat" [matches actor] [name]
				(if (first matches).fail (echo actor (first matches):fail)
					(if (contains actor.channels name)
						(send-to-channel name actor (first matches).text)
						(echo actor "You aren't subscribed to that channel.")
					)
				)
			)
			"Send to the (name) channel."
		)
	)
)

(add-global-verb "subscribe" (m-if-exclusive (m-complete (m-single-word "channel")) (m-nop) (m-fail "Subscribe to what channel?"))
	(lambda "lsubscribe" [matches actor] [] 
		(let ^(^("chat" (load "chat")))
			(let ^(^("channels" (where "channel" chat.channels (equal (first matches).channel channel.name))))
				(if (equal (length channels) 0) (echo actor "That channel does not exist.\n")
					(if ((first channels).can-subscribe actor)
						(nop
							(prop-remove actor "channels" (first matches).channel) /*Prevent duplicate subscriptions*/
							(prop-add actor "channels" (first matches).channel)
							(echo actor "You subscribed to channel ((first matches).channel).\n")
						)
						(echo actor "You can't subscribe to that channel.\n")
					)
				)
			)
		)
	)
	"Subscribe to a channel."
)

(add-global-verb "unsubscribe" (m-if-exclusive (m-complete (m-single-word "channel")) (m-nop) (m-fail "Unsubscribe from what channel?"))
	(lambda "lunsubscribe" [matches actor] []
		(if (contains actor.channels (first matches).channel)
			(nop
				(prop-remove actor "channels" (first matches).channel)
				(echo actor "Unsubscribed.\n")
			)
			(echo actor "You aren't subscribed to that channel.\n")
		)
	)
	"Unsubscribe from a channel."
)

(add-global-verb "channels" (m-if-exclusive (m-nothing) (m-nop) (m-fail "No arguments required.\n"))
	(lambda "lchannels" [matches actor] []
		(for "channel" (load "chat").channels
			(echo actor "(channel.name) (if (contains actor.channels channel.name) ("\(subscribed\)") ("")) (channel.description)\n")
		)
	)
	"List available channels."
)

(add-channel "chat" (lambda "" [actor] [] (true)) "General chat")
(add-channel "wiz" (lambda "" [actor] [] (atleast actor.rank 500)) "Wizard-only chat")
