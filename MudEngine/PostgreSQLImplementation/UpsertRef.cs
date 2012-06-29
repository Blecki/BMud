using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostgreSQLImplementation
{
    /*
     *
-- Function: upsert(bigint, text, text)

-- DROP FUNCTION upsert(bigint, text, text);

CREATE OR REPLACE FUNCTION upsert("object" bigint, "key" text, "value" text)
  RETURNS void AS
$BODY$
BEGIN
    LOOP
        -- first try to update
        UPDATE "Objects" SET "Value" = Value WHERE "Object" = Object AND "Key" = Key;
        -- check if the row is found
        IF FOUND THEN
            RETURN;
        END IF;
        -- not found so insert the row
        BEGIN
            INSERT INTO "Objects" ("Object", "Key", "Value") VALUES (Object, Key, Value);
            RETURN;
            EXCEPTION WHEN unique_violation THEN
                -- do nothing and loop
        END;
    END LOOP;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION upsert(bigint, text, text) OWNER TO "Tony";
*/
}
