using System;

namespace SledSurfers.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UIViewAttribute : Attribute
    {
        public Type View { get; }
        public ViewType Kind { get; }

        public UIViewAttribute(Type view, ViewType kind)
        {
            View = view;
            Kind = kind;
        }
    }
}
