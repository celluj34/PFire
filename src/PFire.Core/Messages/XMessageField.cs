using System;
using System.Text;

namespace PFire.Core.Messages
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class XMessageField : Attribute
    {
        public XMessageField(string name)
        {
            Name = name;
        }

        public XMessageField(params byte[] name) : this(Encoding.UTF8.GetString(name))
        {
            NonTextualName = true;
        }

        public string Name { get; }
        public byte[] NameAsBytes => Encoding.UTF8.GetBytes(Name);
        public bool NonTextualName { get; }
    }
}
