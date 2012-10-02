/* Wearing this object grants the wearer the 'sink' command */

(prop "short" "quantum sink")
(prop "nouns" ^("sink"))
(prop "adjectives" ^("quantum"))
(prop "description" "The sink resembles a wristwatch at first glance, but where it should have a face with spinning hands it instead has a gloweing portal.")

(prop "on-remove" (lambda "" [actor] (echo actor "It seems to be part of your flesh.")))
(prop "can-wear" true)

(add-verb this "sink" (m-complete (m-found-on-worn-by "actor")) (lambda "" [matches actor] (echo actor "Sunk!\n")) "Sink things.")

(prop "create" (lambda "" [] (record ^("@base" (load "quantum-sink")))))