using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace ReflectORM.Extensions
{
    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control c, bool setting)
        {
            Type dgvType = c.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(c, setting, null);
        }
    }
}
