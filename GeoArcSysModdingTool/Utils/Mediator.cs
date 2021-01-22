using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoArcSysModdingTool.Utils
{
    public static class Mediator
    {
        private static readonly IDictionary<string, List<Action<object>>> pl_dict =
            new Dictionary<string, List<Action<object>>>();

        public static void Register(string token, Action<object> callback)
        {
            if (!pl_dict.ContainsKey(token))
            {
                var list = new List<Action<object>>();
                list.Add(callback);
                pl_dict.Add(token, list);
            }
            else if (!pl_dict[token].Any(i => i.Target == callback.Target))
            {
                pl_dict[token].Add(callback);
            }
            else
            {
                var found = false;
                foreach (var item in pl_dict[token])
                    if (item.Method.ToString() == callback.Method.ToString())
                        found = true;
                if (!found)
                    pl_dict[token].Add(callback);
            }
        }

        public static void Unregister(string token, Action<object> callback)
        {
            if (pl_dict.ContainsKey(token)) pl_dict[token].Remove(callback);
        }

        public static void NotifyColleagues(string token, object args)
        {
            if (pl_dict.ContainsKey(token))
                foreach (var callback in pl_dict[token])
                    callback(args);
        }
    }
}