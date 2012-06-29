using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public enum Cardinals
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        Up,
        Down,
        DirectionCount,
    }

    public class Exits
    {
        private static List<String> Names = new List<String>
        { 
            "NORTH", "N",
            "NORTHEAST", "NE",
            "EAST", "E",
            "SOUTHEAST", "SE", 
            "SOUTH", "S",
            "SOUTHWEST", "SW",
            "WEST", "W",
            "NORTHWEST", "NW", 
            "UP", "U",
            "DOWN", "D" 
        };

        public static bool IsCardinal(String _str)
        {
            return Names.Contains(_str.ToUpper());
        }

        public static Cardinals ToCardinal(String _str)
        {
            return (Cardinals)(Names.IndexOf(_str.ToUpper()) / 2);
        }

        public static String ToString(Cardinals Cardinal)
        {
            return Cardinal.ToString();
        }

        public static Cardinals Opposite(Cardinals Cardinal)
        {
            switch (Cardinal)
            {
                case Cardinals.North:
                    return Cardinals.South;
                case Cardinals.NorthEast:
                    return Cardinals.SouthWest;
                case Cardinals.East:
                    return Cardinals.West;
                case Cardinals.SouthEast:
                    return Cardinals.NorthWest;
                case Cardinals.South:
                    return Cardinals.North;
                case Cardinals.SouthWest:
                    return Cardinals.NorthEast;
                case Cardinals.West:
                    return Cardinals.East;
                case Cardinals.NorthWest:
                    return Cardinals.SouthEast;
                case Cardinals.Up:
                    return Cardinals.Down;
                case Cardinals.Down:
                    return Cardinals.Up;
                default :
                    throw new InvalidProgramException();
            }
        }

        public static void CreateLink(MudObject Start, MudObject End, Cardinals Direction)
        {
            Start.SetAttribute(ToString(Direction), End.ID.ToString());
            End.SetAttribute(ToString(Opposite(Direction)), Start.ID.ToString());
        }

        public static Int64 GetLinkTarget(MudObject What, Cardinals Direction)
        {
            try
            {
                return Convert.ToInt64(What.GetAttribute(ToString(Direction), ""));
            }
            catch (Exception)
            {
                return DatabaseConstants.Invalid;
            }
        }

        public static void DestroyLink(MudObject What, Cardinals Direction, IDatabaseService _database)
        {
            Int64 Target = GetLinkTarget(What, Direction);
            var DestObject = MudObject.FromID(Target, _database);
            if (DestObject.Valid) DestObject.DeleteAttribute(ToString(Opposite(Direction)));
            What.DeleteAttribute(ToString(Direction));
        }
    }
}
