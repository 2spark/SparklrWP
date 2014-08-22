using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparklrForWindowsPhone.Helpers
{
    /// <summary>
    /// A class that can be used to check if the code is run at Design Time or at Runtime
    /// </summary>
    public static class DesignTimeHelper
    {
        public static void FailAtRuntime()
        {
            if(!DesignerProperties.GetIsInDesignMode(App.Current.RootVisual))
            {
                throw new InvalidOperationException("This operation is not allowed at runtime");
            }
        }

        public static bool IsDesignTime()
        {
            return DesignerProperties.GetIsInDesignMode(App.Current.RootVisual);
        }
    }
}
