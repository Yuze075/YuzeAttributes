using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
        Inherited = false)]
    public sealed class AddSubclassMenuAttribute : Attribute
    {
        public AddSubclassMenuAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order = order;
        }

        private static readonly char[] Separate = { '/' };
        public string MenuName { get; }
        public int Order { get; }

        public string[] SplitMenuName => !string.IsNullOrWhiteSpace(MenuName)
            ? MenuName.Split(Separate, StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();


        public string TypeNameWithoutPath
        {
            get
            {
                var splitDisplayName = SplitMenuName;
                return splitDisplayName.Length != 0 ? splitDisplayName[^1] : null;
            }
        }
    }
}