using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        private TodoTabNewObserver tabNewTarget;
        private TabJumpingObserver tabJumpingTarget;

        private TemplateView templateView = new TemplateView();

        private TabViewPageModel model = new TabViewPageModel(Application.Current.RealmAccess());

        public TabViewPage()
        {
            #region InitTabView
            void InitTabView()
            {
                var tabs = model.Tabs;
                foreach (var t in tabs)
                {
                    var newTab = new EmptyPage { Setting = t };
                    newTab.Content = this.templateView;
                    Children.Add(newTab);
                }

                //追加ページの位置移動
                if (model.SpecialTabIndex != 0)
                {
                    var specialPage = Children[0];
                    Children.RemoveAt(0);
                    Children.Insert(model.SpecialTabIndex, specialPage);
                }

                //タブバー設定
                BarBackgroundColor = Color.Aquamarine;
                BarTextColor = Color.WhiteSmoke;

                //初期表示タブ
                CurrentPage = Children[model.LastTabIndex];
            }
            #endregion

            InitializeComponent();

            InitTabView();

            tabNewTarget = new TodoTabNewObserver(p => OnTabNew(p));
            tabJumpingTarget = new TabJumpingObserver(t => OnTabJump(t));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget((int)RxSourceEnum.TodoTabNew, tabNewTarget);
            router.AddReactiveTarget((int)RxSourceEnum.TabJumping, tabJumpingTarget);
        }

        private async void OnTabChanged(object sender, EventArgs args)
        {
            if (CurrentPage is EmptyPage page)
            {
                await this.templateView.SetCurrentTodo(page.Setting);
            }

            Title = CurrentPage.Title;
        }

        private async void OnTabNew(string newName)
        {
            //タブIDを発行する
            var todoId = model.GetNewId();

            //タブの規定値を取得
            var setting = await model.GetTabSetting(todoId);
            setting.Name.Value = newName;

            //場所を作る
            var newTab = new EmptyPage { Setting = setting };
            Children.Insert(0, newTab);

            //表示内容設定
            await this.templateView.SetCurrentTodo(setting);
            newTab.Content = this.templateView;
            CurrentPage = Children[0];
        }

        private void OnTabJump(int todoId)
        {
            var tab = Children.OfType<EmptyPage>().Where(p => p.Setting.TodoId.Value == todoId).Select(p => p).FirstOrDefault();
            CurrentPage = tab;
        }
    }
}
