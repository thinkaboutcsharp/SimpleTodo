using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class OriginTabPage : ContentPage
    {
        TodoTabNewObservable source;

        public OriginTabPage()
        {
            InitializeComponent();

            this.source = new TodoTabNewObservable();
            var observableDelegate = (Func<IObserver<TemplatePage>, IDisposable>)(source.Subscribe);
            var observable = Observable.Create<TemplatePage>(observableDelegate);

            var router = (ReactionRouter)Application.Current.Properties[nameof(ReactionRouter)];
            router.AddReactiveSource((int)RxSourceEnum.TodoTabNew, observable);

            txt_TabName.Focus();
        }

        void OnOriginPageTapped(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(txt_TabName.Text))
            {
                DisplayAlert("タブ名", "タブ名を入力してください。", "OK");
                txt_TabName.Focus();
                return;
            }

            var newTab = new TemplatePage(txt_TabName.Text);
            this.source.Send(newTab);
            txt_TabName.Text = string.Empty;
        }

        class TodoTabNewObservable : ObservableBase<TemplatePage>
        {
        }
    }
}
