using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WpfAnimationDemo
{
    public class GetVisualControl
    {
        public static List<T> GetChildrenOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            List<T> list = new List<T>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                //get the current child                 
                if (child is T) list.Add((T)child);
                //if it is of type that you are looking for, then add it to the list        
                list.AddRange(GetChildrenOfType<T>(child)); // on that get a list of children that it has.            
            }
            return list;
        }

        public static T GetChild<T>(DependencyObject parent) where T : DependencyObject
        {
            //if(parent is Label) return parent;             
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T) return (T)child;
                T childLabel = GetChild<T>(child);
                if (childLabel != null) return childLabel;
            } return default(T);
        }
    }
}
