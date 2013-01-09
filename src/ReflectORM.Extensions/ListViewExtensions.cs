using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ReflectORM.Extensions
{
    public static class ListViewExtensions
    {
        public static void DoubleBuffered(this System.Windows.Forms.ListView lv, bool setting)
        {
            Type dgvType = lv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(lv, setting, null);
        }
    }
}
