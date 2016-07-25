using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Player
{
    class Selector : DataTemplateSelector
    {

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item.GetType() == typeof(Track))
            {
                return (DataTemplate)Application.Current.Resources["TrackTemplate"];
            }
            else
            {
                return (DataTemplate)Application.Current.Resources["PlaylistTemplate"];
            }
        }
    }
}
