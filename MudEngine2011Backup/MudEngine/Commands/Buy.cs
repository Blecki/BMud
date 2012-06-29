using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Buy : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);
            var From = What.Location.Parent;

            bool FromIsRoom = From.ID == Actor.Location.Parent.ID;

            var ItemCost = Convert.ToInt32(What.GetLocalAttribute("COST", "1"));
            var LimitedStock = !What.HasLocalAttribute("UNLIMITED");

            if (LimitedStock && Count > What.Count)
            {
                _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, From, What,
                    "^<me:the:the <me:short>> does not have " + Count.ToVerbal() + " <object:plural:<me:short>s>.\n",
                    _database));
                return;
            }

            var TotalCost = ItemCost * Count;

            var PlayerCash = Actor.GetContents("HELD").FirstOrDefault((A) => { return A.ID == DatabaseConstants.Money; });
            if (PlayerCash == null || PlayerCash.Count < TotalCost)
            {
                _message.SendMessage(Actor, "You can't afford that.\n");
                return;
            }

            Actor.RemoveChild(PlayerCash.Location.List, PlayerCash.Location.Index, TotalCost);
            From.AddChild(PlayerCash.Instanciate(TotalCost), "INCOME");

            MudObject PurchasedItem = null;
            if (LimitedStock)
            {
                PurchasedItem = From.UnStack(What.Location.List, What.Location.Index, Count);
                From.RemoveChild(PurchasedItem.Location.List, PurchasedItem.Location.Index, Count);
            }
            else
                PurchasedItem = What.Instanciate(Count);
            PurchasedItem.DeleteLocalAttribute("COST");
            PurchasedItem.DeleteLocalAttribute("UNLIMITED");

            Actor.AddChild(PurchasedItem, "HELD");

            String Message = "<actor:short> buys ";
            if (Count > 1) Message += Count.ToVerbal() + " <me:plural:<me:short>s>";
            else Message += "<me:a:a <me:short>>";
            if (FromIsRoom) Message += ".\n";
            else Message += " from <object:the:the <me:short>>.\n";

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, From, Message, _database, _message);

            String ActorMessage = "You buy ";
            if (Count == 1) ActorMessage += "<me:a:a <me:short>>";
            else ActorMessage += Count.ToVerbal() + " <me:plural:<me:short>s>";
            if (FromIsRoom) ActorMessage += ".\n";
            else ActorMessage += " from <object:the:the <me:short>>.\n";

            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, From, ActorMessage, _database));
        }
    }

    public class Stock : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Store = _match.GetArgument<MudObject>("STORE", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);
            var Cost = _match.GetArgument<Int32>("COST", 1);

            if (!Store.HasAttribute("STORE"))
            {
                _message.SendMessage(Actor, "That is not a store.\n");
                return;
            }

            if (Count <= 0 && MudCore.GetObjectRank(Actor) < 5000)
            {
                _message.SendMessage(Actor, "You can't sell infinite items.\n");
                return;
            }

            bool LimitedStock = Count > 0;

            if (LimitedStock && Count > What.Count)
            {
                _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, What,
                    "You do not have " + Count.ToVerbal() + " <me:plural:<me:short>s>.\n",
                    _database));
                return;
            }

            MudObject ItemForSale = null;
            if (LimitedStock)
            {
                ItemForSale = Actor.UnStack(What.Location.List, What.Location.Index, Count);
                Actor.RemoveChild(ItemForSale.Location.List, ItemForSale.Location.Index, Count);
            }
            else
                ItemForSale = What.Instanciate(Count);

            if (!LimitedStock)
                ItemForSale.SetLocalAttribute("UNLIMITED", "");
            ItemForSale.SetLocalAttribute("COST", Cost.ToString());
            Store.AddChild(ItemForSale, "FORSALE");

            String Message = "<actor:short> stocks <object:the:the <me:short>> with ";
            if (Count == 1) Message += "<me:a:a <me:short>>";
            else if (Count > 1) Message += Count.ToVerbal() + " <me:plural:<me:short>s>";
            else Message += "unlimited <me:plural:<me:short>s>";

            Message += " for sale.\n";
            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, Store, Message, _database, _message);

            Message = "You stock <object:the:the <me:short>> with ";
            if (Count == 1) Message += "<me:a:a <me:short>>";
            else if (Count > 1) Message += Count.ToVerbal() + " <me:plural:<me:short>s>";
            else Message += "unlimited <me:plural:<me:short>s>";
            Message += " for sale.\n";

            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, Store, Message, _database));
        }
    }

    public class Shop : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Store = _match.GetArgument<MudObject>("STORE", null);

            if (!Store.HasAttribute("STORE"))
            {
                _message.SendMessage(Actor, "That is not a store.\n");
                return;
            }

            _message.SendMessage(Actor, "For sale are - \n");

            var ForSale = Store.GetContents("FORSALE");
            foreach (var Item in ForSale)
            {
                String Line = "";
                if (Item.HasLocalAttribute("UNLIMITED")) Line += "     |";
                else
                {
                    Line += Item.Count.ToString();
                    if (Line.Length < 5) Line += new String(' ', 5 - Line.Length);
                    Line += "|";
                }

                String Cost = Item.GetLocalAttribute("COST", "1");
                Line += " $" + Cost;
                if (Cost.Length < 5) Line += new String(' ', 5 - Cost.Length);
                Line += "| ";

                Line += Evaluator.EvaluateString(Actor, Item, Item, "<me:short>", _database);
                Line += "\n";
                _message.SendMessage(Actor, Line);
            }

            if (Actor.Location.Parent.ID == Store.ID)
                _message.SendMessage(Actor, "To buy an item, BUY ITEM.\n");
            else
                _message.SendMessage(Actor,
                    Evaluator.EvaluateString(Actor, Store, Store, "To buy an item, BUY ITEM FROM <me:short>.\n", _database));
        }
    }
}