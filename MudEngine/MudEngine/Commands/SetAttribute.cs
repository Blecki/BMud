using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class SetAttribute : ICommandProcessor
    {
        internal static Dictionary<String, IProtectedAttribute> ProtectedAttributes = 
            new Dictionary<String, IProtectedAttribute>();

        public static void AddProtectedAttribute(String Name, IProtectedAttribute _protectedAttribute)
        {
            ProtectedAttributes.Add(Name.ToUpper(), _protectedAttribute);
        }

        public void Perform(
            PossibleMatch _match, 
            IDatabaseService _database, 
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);
            var Key = _match.GetArgument<String>("KEY", null);
            var Value = _match.GetArgument<String>("VALUE", "");
            var Count = _match.GetArgument<Int32>("COUNT", -1);

            if (!MudCore.CheckPermission(Actor, Target, _database))
                _message.SendMessage(Actor, "You do not have permission to modify that object.\n");
            else
            {
                bool Proceed = true;
                if (ProtectedAttributes.ContainsKey(Key.ToUpper())) Proceed = ProtectedAttributes[Key.ToUpper()].Check
                    (Actor, Target, Value, _message);
              
                if (Proceed)
                {
                    if (Count != -1)
                    {
                        if (!Target.Instance)
                        {
                            _message.SendMessage(Actor, "That is not an instance; do not specify a count.\n");
                            return;
                        }

                        if (Target.Count < Count)
                        {
                            _message.SendMessage(Actor, "There is not " + Count.ToString() + " of those.\n");
                            return;
                        }

                        Target = Target.Location.Parent.UnStack(Target.Location.List, Target.Location.Index, Count);
                    }

                    Target.SetAttribute(Key, Value);
                    if (Target.IsContents(Key)) Target.ReparseContents(Key);
                    _message.SendMessage(Actor, "Attribute set.\n");
                }
            }
        }
    }
}
