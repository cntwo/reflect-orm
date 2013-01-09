using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectORM.Extensions
{
    public static class EventExtensions
    {
        public static void Raise(this EventHandler eventHandler, object sender, EventArgs e)
        {
            if (eventHandler != null)
                eventHandler(sender, e);
        }

        public static void Raise<T>(this EventHandler<T> eventHandler, object sender, T e) where T : EventArgs
        {
            if (eventHandler != null)
                eventHandler(sender, e);
        }
    }
}
