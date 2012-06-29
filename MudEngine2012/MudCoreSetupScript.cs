using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public partial class MudCore
    {
        private void SetupScript()
        {
            scriptEngine.SetupStandardLibrary();

            #region Object Declaration Functions
            scriptEngine.functions.Add("prop", new ScriptFunction("prop", (context, thisObject, arguments) =>
            {
                (thisObject as MudObject).SetAttribute(arguments[0].ToString(), arguments[1]);
                return null;
            }));

            scriptEngine.functions.Add("decor", new ScriptFunction("decor", (context, thisObject, arguments) =>
            {
                var result = new MudObject(database);
                var code = ScriptEvaluater.ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[0]);
                scriptEngine.Evaluate(context, code, result, true);
                return result;
            }));

            #endregion

            #region Debug
            scriptEngine.functions.Add("print", new ScriptFunction("print", (context, thisObject, arguments) =>
                {
                    Console.WriteLine(String.Join(" ", arguments.Select((o, i) => { return o == null ? "NULL" : o.ToString(); })));
                    return null;
                }));
            #endregion

            #region Command Matching
            scriptEngine.functions.Add("verb", new ScriptFunction("verb", (context, thisObject, arguments) =>
                {
                    if (!verbs.ContainsKey(arguments[0].ToString())) verbs.Add(arguments[0].ToString(), new List<Verb>());
                    List<Verb> list = verbs[arguments[0].ToString()];
                    var r = new Verb
                    {
                        Matcher = arguments[1] as ScriptFunction,
                        Action = arguments[2] as ScriptFunction
                    };
                    list.Add(r);
                    return r;
                }));

            scriptEngine.functions.Add("new_match", new ScriptFunction("new_match", (context, thisObject, arguments) =>
                {
                    var from = ScriptEvaluater.ArgumentType<PossibleMatch>(arguments[0]);
                    CommandTokenizer.Token token =
                        arguments[1] == null ? null : ScriptEvaluater.ArgumentType<CommandTokenizer.Token>(arguments[1]);
                    return new PossibleMatch(from.arguments, token);
                }));

            

            //scriptEngine.functions.Add("seq", (context, thisObject, arguments) =>
            //    {
            //        var result = new List<PossibleMatch>();

            //        var Matches = new List<PossibleMatch>();
            //        Matches.Add(_matchState);
            //        foreach (var Matcher in Matchers)
            //        {
            //            var NextMatches = new List<PossibleMatch>();
            //            foreach (var Match in Matches)
            //                NextMatches.AddRange(Matcher.Match(Match, _database, _rawCommand));
            //            Matches = NextMatches;
            //        }
            //        return Matches;


            //    });


            #endregion

            #region Basic Mudding
            scriptEngine.functions.Add("echo", new ScriptFunction("echo", (context, thisObject, arguments) =>
            {
                ScriptEvaluater.ArgumentCount(2, arguments);
                List<Object> to = null;
                if (arguments[0] is List<Object>) to = arguments[0] as List<Object>;
                else
                {
                    to = new List<Object>();
                    to.Add(arguments[0]);
                }
                foreach (var obj in to)
                {
                    var mudObject = obj as MudObject;
                    if (mudObject != null) SendMessage(mudObject, arguments[1].ToString(), false);
                }
                return null;
            }));
            #endregion


        }
    }
}
