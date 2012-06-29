using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public static class DatabaseConstants
    {
        public const int Money = -2;
        public const int StartRoom = 1;
        public const int God = 0;
        public const int Invalid = -3;
        public const int Decor = -1;
        public const Int64 TicksPerDay = 1440;
        public const Int64 TicksPerHour = 60;
        public const Int64 TicksPerMinute = 1;
        public const int RealSecondsPerTick = 10;

    }

    public class Timer
    {
        public Int64 Tick;
        public Int64 ObjectID;
        public String Attribute;
    }

    public interface IDatabaseService
    {
        bool ValidID(Int64 ID);
        void WriteAttribute(Int64 ID, String Key, String Value);
        void RemoveAttribute(Int64 ID, String Key);
        Int64 CreateObject();
        bool DestroyObject(Int64 ID);
        List<Int64> FindObjects(String Key, String Value);

        String QueryAttribute(Int64 ID, String Key, String DefaultValue);
        Dictionary<String, String> GetAllAttributes(Int64 ID);
        bool HasAttribute(Int64 ID, String Key);
        
        void CommitChanges();
        void DiscardChanges();

        void StartTimer(Int64 Tick, Int64 ID, String Value);
        List<Timer> QueryObjectTimers(Int64 ID);
        List<Timer> QueryDueTimers(Int64 Tick);
        void StopTimers(Int64 ID);
        void ClearOldTimers(Int64 Tick);
    }
}
