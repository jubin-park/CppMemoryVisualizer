using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CppMemoryVisualizer.Views
{
    public class MemoryDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            MemoryOwnerInfo memory = item as MemoryOwnerInfo;
            FrameworkElement el = container as FrameworkElement;

            string resourceName = "ValueArrayOnly";

            /*
            if (memory.TypeInfo.Flags == EMemoryTypeFlags.NONE)
            {
                resourceName = "ValueArrayOnly";
            }
            else if (memory.TypeInfo.Flags.HasFlag(EMemoryTypeFlags.POINTER))
            {
                resourceName = "PointerOnly";
            }
            else if (memory.TypeInfo.Flags.HasFlag(EMemoryTypeFlags.ARRAY))
            {
                if (memory.PureTypeInfo.PureName == "int")
                {
                    resourceName = "ValueArrayOnly";
                }
            }
            */

            return (DataTemplate)el.FindResource(resourceName);
        }
    }
}
