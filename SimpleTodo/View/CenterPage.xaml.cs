using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class CenterPage : NavigationPage
    {
        public CenterPage(Page childPage) : base(childPage)
        {
            InitializeComponent();
        }
    }
}
