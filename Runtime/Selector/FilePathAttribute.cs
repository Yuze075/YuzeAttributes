using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FilePathAttribute : PropertyAttribute
    {
        public FilePathAttribute(string relativePath = null)
        {
            RelativePath = relativePath;
        }

        public string RelativePath { get; }
    }
}