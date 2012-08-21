/* Implements an instanced character generation area. Since it's instanced, it can't just be a set of rooms.
	Instead, it's a function that returns a room. */
(depend "door")

(defun "create-character-generation-area" [actor] 
	(let [
		[start-room (record)]
		[scanning-room (record)]
		[locker-room (record)]
		[random-order-number "TK3399R2M"]
		]
		(lastarg

			(lfun "voice-sequence" [actor]
				(if actor.creation-finished (nop)
					(nop
						(echo actor "The voice says \"Proceed east from the locker room to the termination center for recycling.\"\n")
						(invoke 10 voice-sequence actor)
					)
				)
			)

			(lfun "scan-sequence-four" [actor]
				(nop
					(echo actor "The voice says \"Order unacceptable. Continue to termination center for recycling.\"\n")
					(echo actor "The door at the end of the hall slides open.\n")
					(set scanning-room.door "open" true)
					(set scanning-room.door "locked" false)
					(invoke 10 voice-sequence actor)
				)
			)

			(lfun "scan-sequence-three" [actor] 
				(nop
					(echo actor "The voice continues \"Actual Specification: (actor.gender), (
						(load "stats").calculate-height actor.age actor.height actor.gender) inches, body type: (actor.body-type).\"\n")
					(invoke 5 scan-sequence-four actor)
				)
			)					

			(lfun "scan-sequence-two" [actor] (nop
				(echo actor "A mechanical voice recites \"Order (random-order-number) Specification: Male, 72 inches, body type: Muscular. Intelligence: N/A. Trainability: 12. Independence: 0.\"\n")
				(invoke 5 scan-sequence-three actor)
			))

			(lfun "scan-sequence-one" [actor] 
				(nop
					(echo actor "A laser shoots from the far end of the hall and sweeps up and down your body.\n")
					(invoke 5 scan-sequence-two actor)
				)
			)
			
			(lfun "start-scan-sequence" [actor] 
				(nop
					((load "stats").generate-character-stats actor)
					(invoke 5 scan-sequence-one actor)
				)
			)

			(multi-set start-room [
				[@base (load "room")]
				[short "Dank tiled stall"]
				[long "A thousand streams of dirty water run down the walls, which might once have been white tile, to a drain nearly clogged with muck and slime. A single florescent light flickers overhead."]
				])
			(open-direct-link start-room scanning-room ^("left" "l" "girl") 
				(lambda "lchoose-girl" [actor] 
					(nop 
						(echo actor "You have chosen to be a girl.")
						(set actor "gender" "female")
						(set actor "pronoun" "she")
						(start-scan-sequence actor)
					))
			)
			(open-direct-link start-room scanning-room ^("right" "r" "boy") 
				(lambda "lchoose-boy" [actor] 
					(nop
						(echo actor "You have chosen to be a boy.")
						(set actor "gender" "male")
						(start-scan-sequence actor)
					)
				)
			)
			(add-object start-room "contents" (record 
				^("@base" (load "object"))
				^("short" "dirty sign")
				^("nouns" ^("sign"))
				^("adjectives" ^("dirty"))
				^("description" "The sign says, girls to the left, boys to the right.")
				^("can-read" true) /* Allow the 'read' verb to be applied to the sign. */
				^("on-get" (lambda "" [actor] (echo actor "That appears to be attached to the wall.\n")))
			))

			(multi-set scanning-room [
				[@base (load "room")]
				[short "Glass hallway"]
				[long "Dark glass lines both sides of this narrow hallway. You can't see through it, either because it is too filthy or because the other side is dark."]
				])

			(multi-set locker-room [
				[@base (load "room")]
				[short "Musty Locker Room"]
				[long ""]
				])

			(set scanning-room "door" (create-direct-door scanning-room locker-room ^("north" "n")))
			(multi-set scanning-room.door [
				["locked" true]
				["short" "steel door"]
				["adjectives" ^("steel")]
				])

			/* Clothe the player */
			(add-object actor "worn" (record
				^("@base" (load "object"))
				^("short" "paper gown")
				^("nouns" ^("gown"))
				^("adjectives" ^("paper"))
				^("description" "This is a slightly crumpled paper gown that ties closed in the back.")
				^("can-wear" true)
			))

			(set locker-room "in-door" (create-direct-door locker-room null ^("south" "s")))
			(multi-set locker-room.in-door [
				["locked" true]
				["short" "steel door"]
				["adjectives" ^("steel")]
				])

			(multi-set (create-direct-door locker-room null ^("east" "e")) [
				["locked" true]
				["short" "striped door"]
				["description" "This door is painted in diagonal yellow stripes."]
				["adjectives" ^("striped")]
				])

		start-room)
	)
)
