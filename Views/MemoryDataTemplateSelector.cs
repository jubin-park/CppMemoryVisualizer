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

            var pureType = PureTypeManager.GetType(memory.TypeInfo.PureName);

            string resourceName;
            if (memory is HeapMemoryInfo)
            {
                resourceName = "HeapPrimitive";
            }
            else
            {
                if (pureType.Flags.HasFlag(EMemoryTypeFlags.CLASS) || pureType.Flags.HasFlag(EMemoryTypeFlags.STRUCT))
                {
                    resourceName = "StructOrClass";
                }
                else
                {
                    resourceName = "StackPrimitive";
                }
            }

            return (DataTemplate)el.FindResource(resourceName);
        }
    }
}
