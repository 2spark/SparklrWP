
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace SparklrForWindowsPhone.Helpers
{
    public class JumpListFirstItemTemplateSelector : DataTemplateSelector
    {
        bool isFirst = true;

        public DataTemplate FirstItemTemplate
        {
            get;
            set;
        }

        public DataTemplate StandardItemTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (isFirst)
            {
                isFirst = false;
                return this.FirstItemTemplate;
            }
            else
            {
                return this.StandardItemTemplate;
            }

        }
    }
}
