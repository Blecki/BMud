using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class MUL : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            var Result = new Integer(1);

            foreach (var Child in Children)
            {
                var Object = Child.Execute(Context);
                if (Object is Integer)
                    Result.Value *= (Object as Integer).Value;
                else if (Object is String)
                {
                    try
                    {
                        Result.Value *= Convert.ToInt32(Object as String);
                    }
                    catch (Exception)
                    {
                        throw new RuntimeErrorException("Could not convert string to integer.");
                    }
                }
                else
                    throw new RuntimeErrorException("Type error in MUL.");
            }

            return Result;
        }
    }
}
