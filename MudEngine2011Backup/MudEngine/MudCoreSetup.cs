using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MudEngine.Matchers;
using MudEngine.Commands;

namespace MudEngine
{
    public partial class MudCore
    {
        public MudCore(IAccountService _accountService, IDatabaseService _databaseService)
        {
            AccountService = _accountService;
            DatabaseService = _databaseService;

            Parser.AddHelpTopic("BUILD COMMANDS", "In build commands, you can refer to objects by the ID. Prefix the ID with #, as in 'teleport me #1'\n");
            Parser.AddHelpTopic("RANK", "Every object has a rank. Rank determines what commands are available to an object. Normal players have a rank of 0. A rank of 5000 makes build commands available.\n");
            Parser.AddHelpTopic("OWNERSHIP", "Every object has an owner. To be able to modify an object, you must own it, or outrank it's owner. You can change an object's owner using the SetOwner command, but be careful - you might not be able to modify it anymore afterwards!\n");
            Parser.AddHelpTopic("DISPLAYIDS", "Players of rank 5000 or higher can give themselves the 'DISPLAYIDS' attribute, which will display object IDs when they look at a room.\n");
            //Parser.AddHelpTopic("INHERITENCE", "Every object has a parent. When the engine looks for an attribute on an object, if it can't find it, it will look on it's parent. An object's parent must be earlier in the database than it. All objects inherit from object 0 by default.\n");

            Parser.AddHelpTopic("DETAILS", "Some rooms have details. You can look at them, but you have to figure out what they are. There should be clues in the room description.\n");

            Parser.AddCommandSet("LOOK", new CardinalDirection("DIRECTION"), new LookDirection());
            Parser.AddCommandSet("LOOK", new Sequence(new KeyWord("AT", true),
                new Flipper(
                    new ObjectM(new OSContentsSpec("WHERE", "LIST"), "OBJECT"),
                    new InOnUnder("LIST"),
                    new OrMatcher(
                        new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                        new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE")))),
                    new LookAt());
            Parser.AddCommandSet("LOOK", new Sequence(new KeyWord("AT", true),
                new Flipper(
                    new Rest("REST"),
                    new InOnUnder("LIST"),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                    new SimpleEcho("I don't see that there.\n"));
            Parser.AddCommandSet("LOOK", new Sequence(
                new InOnUnder("LIST"),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "OBJECT"))),
                new LookIn());
            Parser.AddCommandSet("LOOK", new KeyWord("AT", false), new SimpleEcho("Look at what?\n"));
            Parser.AddCommandSet("LOOK", new Sequence(new KeyWord("AT", true), new KeyWord("HERE", true)),
                new LookHere());
            Parser.AddCommandSet("LOOK", new Sequence(new KeyWord("AT", true), new KeyWord("ME", false)),
                new LookMe());
            Parser.AddCommandSet("LOOK", new Sequence(
                new KeyWord("AT", true),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                    new ObjectM(new OSEverything("ACTOR"), "OBJECT"))),
                new LookAt());
            Parser.AddCommandSet("LOOK", new DetailMatcher(), new LookDetail());
            Parser.AddCommandSet("LOOK", new Rest("REST"), new SimpleEcho("I don't see that.\n"));
            Parser.SetCommandRank("LOOK", 0);
            Parser.AddHelpTopic("LOOK",
                "Look [at] [Object]\n" +
                "Look at an object. Without arguments, the command is the same as 'look here'.\n" +
                "Me is a shortcut for yourself, as in 'look me'.\n");

            Parser.AddCommandSet("CREATE", new Rest("REST"), new Create());
            Parser.AddCommandSet("CREATE", new None(), new SimpleEcho("You have to tell me what to call it.\n"));
            Parser.SetCommandRank("CREATE", 5000);
            Parser.AddHelpTopic("CREATE", "Create [New object name] : Requires rank 5000. Creates a new object. The object will be placed in your inventory.\n");

            Parser.AddCommandSet("RECYCLE", new ObjectID("OBJECT"), new Recycle());
            Parser.SetCommandRank("RECYCLE", 9000);

            Parser.AddCommandSet("DIG", new Sequence(
                new CardinalDirection("DIRECTION"), new Rest("REST")), new DigDirection());
            Parser.AddCommandSet("DIG", new Rest("REST"), new Dig());
            Parser.AddCommandSet("DIG", new None(), new SimpleEcho("At least specify the name of the new room.\n"));
            Parser.SetCommandRank("DIG", 5000);
            Parser.AddHelpTopic("DIG", "Dig [Cardinal] Short : Requires rank 5000. Creates a new room, optionally linking in the specified cardinal direction.\n");

            Parser.AddCommandSet("LINK", new Sequence(
                new CardinalDirection("DIRECTION"), new ObjectID("OBJECT")),
                new Link());
            Parser.SetCommandRank("LINK", 5000);
            Parser.AddHelpTopic("LINK", "Link Cardinal Room : Requires rank 5000. Open a link between your current location and another room.\n");

            Parser.AddCommandSet("UNLINK", new CardinalDirection("DIRECTION"), new UnLink());
            Parser.SetCommandRank("UNLINK", 5000);
            Parser.AddHelpTopic("UNLINK", "Unlink Cardinal : Requires rank 5000. Destroy a link.\n");

            Parser.AddCommandSet("EXAMINE", 
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                    new AllObjects("OBJECT")), new Examine());
            Parser.AddCommandSet("EXAMINE", new Rest("REST"), new SimpleEcho("Examine what?\n"));
            Parser.SetCommandRank("EXAMINE", 5000);
            Parser.AddHelpTopic("EXAMINE", "Examine Object : Requires rank 5000. View details of an object.\n");

            Parser.AddCommandSet("SETATTR",
                new Sequence(
                    new Optional(new Sequence(new KeyWord("ON", false), new IntegerM("COUNT"))),
                    new OrMatcher(
                        new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                        new AllObjects("OBJECT")),
                    new SingleWord("KEY"), new Optional(new Rest("VALUE"))), new SetAttribute());
            Parser.SetCommandRank("SETATTR", 5000);
            Parser.AddHelpTopic("SETATTR", "Setattr Object Key [Value] : Requires rank 5000. Set an attribute on an object.\n");

            Parser.AddCommandSet("DELATTR", 
                new Sequence(
                    new Optional(new Sequence(new KeyWord("ON", false), new IntegerM("COUNT"))),
                    new OrMatcher(
                        new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                        new AllObjects("OBJECT")), 
                    new SingleWord("KEY")), new DeleteAttribute());
            Parser.SetCommandRank("DELATTR", 5000);
            Parser.AddHelpTopic("DELATTR", "Delattr Object Key : Requires rank 5000. Remove an attribute from an object.\n");

            Parser.AddCommandSet("TELEPORT", new Sequence(
                new OrMatcher(
                    new ObjectID("OBJECT"),
                    new Me("OBJECT")),
                new OrMatcher(
                    new ObjectID("WHERE"),
                    new Me("WHERE"),
                    new Here("WHERE")),
                new SingleWord("LIST")),
                new Teleport());
            Parser.SetCommandRank("TELEPORT", 5000);
            Parser.AddHelpTopic("TELEPORT", "Teleport Object Object ListName : Requires rank 5000. Telport an object from one location to another. This operation is silent and unrestricted, be careful.\n");

            Parser.AddHelpTopic("INSTANCES", "An INSTANCE object is an object that can be many places at once. The effect is that, from the perspective of a player, there can be many copies of the object, without there having to be many copies in the database. You can turn an object into an INSTANCE object using the MAKEINSTANCE command. This is a one-way transformation. Use the INSTANCE command to make a new copy of the object, and the BANISH command to be rid of a copy you are holding.\n");

            Parser.AddCommandSet("INSTANCE", new Sequence(new Optional(new IntegerM("COUNT")),
                new OrMatcher(
                    new ObjectID("OBJECT"), 
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "OBJECT")),
                    new ObjectM(new OSEverything("ACTOR"), "OBJECT"))), 
                new Instance());
            Parser.SetCommandRank("INSTANCE", 5000);
            Parser.AddHelpTopic("INSTANCE", "Instance ID : Requires rank 5000. Create a new instance of an INSTANCE object. See topic INSTANCES\n");

            Parser.AddCommandSet("DECOR", new Rest("REST"), new Decor());
            Parser.SetCommandRank("DECOR", 5000);

            Parser.AddCommandSet("BANISH", new Sequence(new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT")), new Banish());
            Parser.SetCommandRank("BANISH", 5000);
            Parser.AddHelpTopic("BANISH", "Banish Object : Requires rank 5000. Destroy an instance of an INSTANCE object that you are holding. See topic INSTANCES\n");

            Parser.AddCommandSet("EVAL", new Rest("REST"), new Evaluate());
            Parser.AddCommandSet("EVAL", new None(), new SimpleEcho("Evaluate what?\n"));
            Parser.SetCommandRank("EVAL", 5000);
            Parser.AddHelpTopic("EVAL", "Eval String : Requires rank 5000. Evaluate a string using the engine's attribute evaluator.\n");

            Parser.AddCommandSet("SHOWPARSE", new Sequence(new ObjectM(new OSEverything("ACTOR"), "OBJECT"),
                new SingleWord("ATTRIBUTE")), new ShowParseTree());
            Parser.SetCommandRank("SHOWPARSE", 5000);

            Parser.AddCommandSet("FORCE", new Sequence(new ObjectID("OBJECT"), new Rest("REST")), new Force());
            Parser.SetCommandRank("FORCE", 8000);

            Parser.AddCommandSet("FIND", new Sequence(
                new SingleWord("KEY"), new Rest("VALUE")), new Query());
            Parser.AddCommandSet("FIND", new SingleWord("KEY"), new Query());
            Parser.SetCommandRank("FIND", 5000);
            Parser.AddHelpTopic("FIND", "Find Key Value : Requires rank 5000. Lists all objects with the specified key/value pair.\n");

            Parser.AddCommandSet("TIMERS", new ObjectID("OF"), new QueryTimers());
            Parser.SetCommandRank("TIMERS", 5000);

            Parser.AddCommandSet("STOPTIMERS", new ObjectID("OF"), new StopTimers());
            Parser.SetCommandRank("STOPTIMERS", 5000);

            Parser.AddCommandSet("SAY", new Rest("REST"), new Say());
            Parser.AddCommandSet("SAY", new None(), new SimpleEcho("Say what?\n"));
            Parser.SetCommandRank("SAY", 0);
            Parser.AddCommandAlias("'", "SAY");
            Parser.AddHelpTopic("SAY", "Say String : Say something!\n");

            Parser.AddCommandSet("EMOTE", new Rest("REST"), new Emote());
            Parser.AddCommandSet("EMOTE", new None(), new SimpleEcho("Eh?\n"));
            Parser.SetCommandRank("EMOTE", 0);
            Parser.AddHelpTopic("EMOTE", "Emote String : Pretty much just a simple echo.\n");

            Parser.AddCommandSet("ASK", new Sequence(new Sequence(
                new ObjectM(new OSLocation("ACTOR", "IN"), "WHO"),
                new KeyWord("ABOUT", true)), new SingleWord("TEXT")), new Ask());
            Parser.SetCommandRank("ASK", 0);

            Parser.AddCommandSet("GET", new Flipper(
                new Sequence(
                    new Optional(new IntegerM("COUNT")),
                    new ObjectM(new OSContentsSpec("WHERE", "LIST"), "OBJECT")),
                new Sequence(new KeyWord("FROM", true), new InOnUnder("LIST")),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                new Get());
            Parser.AddCommandSet("GET", new Flipper(
                new Sequence(
                    new Optional(new IntegerM("COUNT")),
                    new ObjectM(new OSContents("WHERE", "IN:ON"), "OBJECT")),
                new KeyWord("FROM", false), 
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                new Get());
            Parser.AddCommandSet("GET", new Flipper(new Rest("REST"),
                new Sequence(new KeyWord("FROM", true), new InOnUnder("LIST")),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                new SimpleEcho("I don't see that there.\n"));
            Parser.AddCommandSet("GET", new Flipper(new Rest("REST"),
                new Sequence(new KeyWord("FROM", true), new InOnUnder("LIST")),
                new Rest("REST")), 
                new SimpleEcho("I don't see that here.\n"));
            Parser.AddCommandSet("GET", new Sequence(
                new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSLocation("ACTOR", "IN"), "OBJECT")),
                new Get());
            Parser.AddCommandSet("GET", new Sequence(
                new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSOnLocationContents("ACTOR"), "OBJECT")),
                new Get());
            Parser.AddCommandSet("GET", new Rest("REST"), new SimpleEcho("I don't see that here.\n"));
            Parser.AddCommandSet("GET", new None(), new SimpleEcho("Get what?\n"));
            Parser.SetCommandRank("GET", 0);
            Parser.AddHelpTopic("GET", "Get Object : Pickup an object.\n");
            Parser.AddCommandAlias("TAKE", "GET");


            Parser.AddCommandSet("DROP", new Sequence(
                new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"),
                new InOnUnder("LIST"),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                new Drop());
            Parser.AddCommandSet("DROP", new Sequence(
                new ObjectM(new OSContents("ACTOR", "WORN"), "OBJECT"),
                new InOnUnder("LIST"),
                new OrMatcher(
                    new Sequence(new KeyWord("MY", false), new ObjectM(new OSContents("ACTOR", "HELD:WORN"), "WHERE")),
                    new ObjectM(new OSSHeldWornLoc("ACTOR"), "WHERE"))),
                new Chain(new Remove(), new Drop()));
            Parser.AddCommandSet("DROP", new Sequence(
                new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT")),
                new Drop());
            Parser.AddCommandSet("DROP", new ObjectM(new OSContents("ACTOR", "WORN"), "OBJECT"),
                new Chain(new Remove(), new Drop()));
            Parser.AddCommandSet("DROP", new Rest("REST"), new SimpleEcho("You don't have that.\n"));
            Parser.AddCommandSet("DROP", new None(), new SimpleEcho("Drop what?\n"));
            Parser.SetCommandRank("DROP", 0);
            Parser.AddHelpTopic("DROP", "Drop Object : Drop an object.\n");
            Parser.AddHelpTopic("PUT", "Put Object in/on/under Object : Put an object in, on, or under another one, if allowed.\n");
            Parser.AddCommandAlias("PUT", "DROP");

            Parser.AddCommandSet("GIVE", new Sequence(
                new Optional(new IntegerM("COUNT")),
                new Sequence(new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"),
                new Sequence(new KeyWord("TO", false),
                    new ObjectM(new OSLocation("ACTOR", "IN"), "TO")))), new Give());
            Parser.SetCommandRank("GIVE", 0);

            Parser.AddCommandSet("BUY", new Sequence(new Optional(new IntegerM("COUNT")), new Flipper(
                new ObjectM(new OSContents("FROM", "FORSALE"), "OBJECT"),
                new KeyWord("FROM", false), new ObjectM(new OSLocation("ACTOR", "IN"), "FROM"))), new Buy());
            Parser.AddCommandSet("BUY", new Sequence(new Optional(new IntegerM("COUNT")), new Flipper(
               new Rest("REST"),
               new KeyWord("FROM", false), new ObjectM(
                   new OSLocation("ACTOR", "IN"), "FROM"))), new SimpleEcho("That doesn't seem to be for sale.\n"));
            Parser.AddCommandSet("BUY", new Sequence(new Optional(new IntegerM("COUNT")),
                new ObjectM(new OSLocation("ACTOR", "FORSALE"), "OBJECT")), new Buy());
            Parser.AddCommandSet("BUY", new Rest("REST"), new SimpleEcho("That doesn't seem to be for sale.\n"));
            Parser.SetCommandRank("BUY", 0);

            Parser.AddCommandSet("STOCK", new Sequence(
                new OrMatcher(new Here("STORE"), new ObjectM(new OSLocation("ACTOR", "IN"), "STORE")),
                new KeyWord("WITH", false),
                new Optional(new IntegerM("COUNT")),
                new Sequence(
                    new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"),
                    new IntegerM("COST"))),
                new Stock());
            Parser.SetCommandRank("STOCK", 5000);

            Parser.AddCommandSet("SHOP", new OrMatcher(
                new Here("STORE"),
                new ObjectM(new OSLocation("ACTOR", "IN"), "STORE")),
                new Shop());
            Parser.AddCommandSet("SHOP", new Sequence(new None(),
                new Push("STORE", (A) => { return A.GetArgument<MudObject>("ACTOR", null).Location.Parent; })),
                new Shop());
            Parser.AddCommandSet("SHOP", new Rest("REST"), new SimpleEcho("I don't see that here.\n"));
            Parser.SetCommandRank("SHOP", 0);

            Parser.AddCommandSet("GO", new CardinalDirection("DIRECTION"), new Go());
            Parser.AddCommandSet("GO", new Rest("REST"), new SimpleEcho("That is not a direction.\n"));
            Parser.SetCommandRank("GO", 0);
            Parser.AddHelpTopic("GO", "Go Exit : Move around the world.\n");

            Parser.AddCommandSet("WEAR", new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"), new Wear());
            Parser.AddCommandSet("WEAR", new Rest("REST"), new SimpleEcho("You don't seem to be holding that.\n"));
            Parser.AddCommandSet("WEAR", new None(), new SimpleEcho("Wear what?\n"));
            Parser.SetCommandRank("WEAR", 0);
            Parser.AddHelpTopic("WEAR", "Wear Object : If an object is clothing, wear it.");

            Parser.AddCommandSet("REMOVE", new ObjectM(new OSContents("ACTOR", "WORN"), "OBJECT"), new Remove());
            Parser.AddCommandSet("REMOVE", new Rest("REST"), new SimpleEcho("You don't seem to be wearing that.\n"));
            Parser.AddCommandSet("REMOVE", new None(), new SimpleEcho("Remove what?\n"));
            Parser.SetCommandRank("REMOVE", 0);
            Parser.AddHelpTopic("REMOVE", "Remove Object : Remove something you are wearing.\n");

            Parser.AddCommandSet("DRINK", new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"), new Drink());
            Parser.AddCommandSet("DRINK", new Rest("REST"), new SimpleEcho("You don't seem to holding that.\n"));
            Parser.AddCommandSet("DRINK", new None(), new SimpleEcho("Drink what?\n"));
            Parser.SetCommandRank("DRINK", 0);

            Parser.AddCommandSet("EAT", new ObjectM(new OSContents("ACTOR", "HELD"), "OBJECT"), new Eat());
            Parser.AddCommandSet("EAT", new Rest("REST"), new SimpleEcho("You don't seem to holding that.\n"));
            Parser.AddCommandSet("EAT", new None(), new SimpleEcho("Eat what?\n"));
            Parser.SetCommandRank("EAT", 0);


            Parser.AddDefaultCommand(new CardinalDirection("DIRECTION"), new Go());

            //Commands.SetAttribute.AddProtectedAttribute("PARENT", new ProtectedAttributes.Parent());
            Commands.SetAttribute.AddProtectedAttribute("RANK", new ProtectedAttributes.Rank());
            Commands.SetAttribute.AddProtectedAttribute("OWNER", new ProtectedAttributes.MustBeInteger());
            Commands.SetAttribute.AddProtectedAttribute("LOCATION", new ProtectedAttributes.MustBeRank(9000));
            Commands.SetAttribute.AddProtectedAttribute("IN", new ProtectedAttributes.MustBeRank(9000));
            Commands.SetAttribute.AddProtectedAttribute("ON", new ProtectedAttributes.MustBeRank(9000));
            Commands.SetAttribute.AddProtectedAttribute("WORN", new ProtectedAttributes.MustBeRank(9000));
            Commands.SetAttribute.AddProtectedAttribute("HELD", new ProtectedAttributes.MustBeRank(9000));
            Commands.SetAttribute.AddProtectedAttribute("DATABASE_NEXT_OBJECT_ID", new ProtectedAttributes.NextObjectID());
            Commands.SetAttribute.AddProtectedAttribute("SHORT", new ProtectedAttributes.NeverEmpty());
            Commands.SetAttribute.AddProtectedAttribute("HEARTBEAT", new ProtectedAttributes.MustBeRank(9000));

            Evaluator.AddFunction("shortlist", Evaluator.FuncShortlist);
            Evaluator.AddFunction("actor", Evaluator.FuncActor);
            Evaluator.AddFunction("me", Evaluator.FuncMe);
            Evaluator.AddFunction("object", Evaluator.FuncObject);
        }
    }
}
