using System;
using System.Collections.Generic;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        private TodoTabNewObserver target;

        public TabViewPage()
        {
            InitializeComponent();

            target = new TodoTabNewObserver(p => OnTabNew(p));
            var router = (ReactionRouter)Application.Current.Properties[nameof(ReactionRouter)];
            router.AddReactiveTarget<TemplatePage>((int)RxSourceEnum.TodoTabNew, target);
        }

        void OnTabChanged(object sender, EventArgs args)
        {
            Title = CurrentPage.Title;
        }

        void OnTabNew(TemplatePage newTab)
        {
            Children.Insert(0, newTab);
            CurrentPage = newTab;
        }

        class TodoTabNewObserver : ObserverBase<TemplatePage>
        {
            public TodoTabNewObserver(Action<TemplatePage> action) : base(action) { }
        }
    }
}
