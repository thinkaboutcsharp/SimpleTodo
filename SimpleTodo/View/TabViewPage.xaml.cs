using System;
using System.Collections.Generic;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        private TodoTabNewObserver tabNewTarget;

        private TemplateView templateView = new TemplateView();

        private TabViewPageModel model = new TabViewPageModel();

        public TabViewPage()
        {
            InitializeComponent();

            tabNewTarget = new TodoTabNewObserver(p => OnTabNew(p));

            var router = (ReactionRouter)Application.Current.Properties[nameof(ReactionRouter)];
            router.AddReactiveTarget<string>((int)RxSourceEnum.TodoTabNew, tabNewTarget);
        }

        void OnTabChanged(object sender, EventArgs args)
        {
            if (CurrentPage is EmptyPage page)
            {
                this.templateView.SetCurrentTodo(page.Tab);
            }

            Title = CurrentPage.Title;
        }

        void OnTabNew(string newName)
        {
            //タブIDを発行する
            var todoId = model.GetNewId();

            //タブの規定値を取得
            var setting = model.GetTabSetting(todoId);
            setting.Name = newName;

            //場所を作る
            var newTab = new EmptyPage { Tab = setting };
            Children.Insert(0, newTab);

            //表示内容設定
            this.templateView.SetCurrentTodo(setting);
            newTab.Content = this.templateView;
            CurrentPage = Children[0];
        }

        class TodoTabNewObserver : ObserverBase<string>
        {
            public TodoTabNewObserver(Action<string> action) : base(action) { }
        }
    }
}
