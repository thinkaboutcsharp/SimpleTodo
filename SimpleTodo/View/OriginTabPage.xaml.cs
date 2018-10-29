using System;
using System.Collections.Generic;
using System.Reactive.Linq;

using Xamarin.Forms;
using Anywhere;

namespace SimpleTodo
{
    public partial class OriginTabPage : ContentPage
    {
        TodoTabNewObservable source = new TodoTabNewObservable();

        public OriginTabPage()
        {
            InitializeComponent();

            var router = Application.Current.ReactionRouter();
            router.AddReactiveSource(RxSourceEnum.TodoTabNew, source);

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

            this.source.Send(txt_TabName.Text);
            txt_TabName.Text = string.Empty;
        }
    }
}
