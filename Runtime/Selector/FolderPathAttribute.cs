using System;
using System.Diagnostics;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FolderPathAttribute : PropertyAttribute
    {
        public FolderPathAttribute(string relativePath = null)
        {
            RelativePath = relativePath;
        }

        public string RelativePath { get; }
    }
}