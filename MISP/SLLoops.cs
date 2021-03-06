﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupLoopFunctions()
        {
            functions.Add("map", new Function("map",
                ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"), 
                "variable_name list code : Transform one list into another",
                (context, arguments) =>
                {
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var result = new ScriptList();
                    context.Scope.PushVariable(vName, null);
                    foreach (var item in list)
                    {
                        context.Scope.ChangeVariable(vName, item);
                        result.Add(Evaluate(context, code, true));
                    }
                    context.Scope.PopVariable(vName);
                    return result;
                }));

            functions.Add("mapi", new Function("mapi",
                ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"),
                "variable_name list code : Like map, except variable_name will hold the index not the value.",
                (context, arguments) =>
                {
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var result = new ScriptList();
                    context.Scope.PushVariable(vName, null);
                    for (int i = 0; i < list.Count; ++i)
                    {
                        context.Scope.ChangeVariable(vName, i);
                        result.Add(Evaluate(context, code, true));
                    }
                    context.Scope.PopVariable(vName);
                    return result;
                }));

            functions.Add("mapex", new Function("mapex",
                ArgumentInfo.ParseArguments(this, "string variable_name", "start", "code code", "code next"), 
                "variable_name start code next : Like map, but the next element is the result of 'next'. Stops when next = null.",
                (context, arguments) =>
                {
                    var vName = ArgumentType<String>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var next = ArgumentType<ParseNode>(arguments[3]);
                    var result = new ScriptList();
                    var item = arguments[1];
                    context.Scope.PushVariable(vName, null);

                    while (item != null)
                    {
                        context.Scope.ChangeVariable(vName, item);
                        result.Add(Evaluate(context, code, true));
                        item = Evaluate(context, next, true);
                    }

                    context.Scope.PopVariable(vName);
                    return result;
                }));

            functions.Add("for", new Function("for",
                ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"), 
                "variable_name list code : Execute code for each item in list. Returns result of last run of code.",
                (context, arguments) =>
                {
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<ParseNode>(arguments[2]);
                    context.Scope.PushVariable(vName, null);
                    Object result = null;
                    foreach (var item in list)
                    {
                        context.Scope.ChangeVariable(vName, item);
                        result = Evaluate(context, func, true);
                    }

                    context.Scope.PopVariable(vName);

                    return result;
                }));

            functions.Add("while", new Function("while",
                ArgumentInfo.ParseArguments(this, "code condition", "code code"),
                "condition code : Repeat code while condition evaluates to true.",
                (context, arguments) =>
                {
                    var cond = ArgumentType<ParseNode>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[1]);

                    while (Evaluate(context, cond, true) != null)
                        Evaluate(context, code, true);
                    return null;
                }));

            functions.Add("repeat", new Function("repeat",
                ArgumentInfo.ParseArguments(this, "integer count", "code code"),
                "count code: Repeat code count times. If you're looking for a indexed for loop, try mapex.",
                (context, arguments) =>
                {
                    var count = arguments[0] as int?;
                    if (count == null || !count.HasValue) return null;
                    var _count = count.Value;
                    while (_count > 0)
                    {
                        Evaluate(context, arguments[1] as ParseNode, true, true);
                        _count -= 1;
                    }
                    return null;
                }));
        }
    }
}
