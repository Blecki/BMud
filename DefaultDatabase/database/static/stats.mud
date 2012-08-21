/* Vital statistics for all characters */

(let ^(^("_this" this))
	(nop
(prop "lower-age" 8)
(prop "upper-age" 20)

(prop "female-height" ^(50 52 54 56 58 60 62 63 64 64 64 64 64 64)) /* Base line height in inches. The first entry is lower-age, */
(prop "male-height" ^(50 52 54 56 58 60 62 63 64 68 69 70 70 70))   /* characters older than upper-age no longer grow. */

(prop "body-types" ^("slim" "muscular" "soft"))

(prop "height-variance" ^(-5 5))

(prop "calculate-height" (lambda "" [age variance gender] 
	(if (atleast age _this.upper-age) (add (last _this."(gender)-height") variance)
		(add (index _this."(gender)-height" (subtract age _this.lower-age)) variance)
	))
)

(prop "calculate-actual-height" (lambda "" [actor] ((load "stats").calculate-height actor.age actor.height actor.gender)))

(prop "generate-character-stats" (lambda "" [character]
	(nop
		(set character "age" 8)
		(set character "height" (random (first _this.height-variance) (last _this.height-variance)))
		(set character "body-type" (index _this.body-types (random 0 3)))
	)
))
	)
)