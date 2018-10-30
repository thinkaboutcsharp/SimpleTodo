using System;
using System.Collections.Generic;
using stt = System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;
using Anywhere;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        private IReactiveSource<TodoItem> tabChangeSource;

        private TemplateView templateView = new TemplateView();

        private TabViewPageModel model = new TabViewPageModel(Application.Current.DataAccess());

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

            var router = Application.Current.ReactionRouter();
            tabChangeSource = router.AddReactiveSource<TodoItem>(RxSourceEnum.CentralViewChange);
            router.AddReactiveTarget(RxSourceEnum.TodoTabNew, (string t) => OnTabNew(t));
            router.AddReactiveTarget(RxSourceEnum.TabJumping, (int t) => OnTabJump(t));
            router.AddReactiveTarget(RxSourceEnum.TabUpDown, async ((UpDown, int) m) => await OnTabUpDown(m));
            router.AddReactiveTarget(RxSourceEnum.TodoTabVisibleChange, async ((int, bool) v) => await OnChangeVisibility(v));
            router.AddReactiveTarget(RxSourceEnum.TabTitleChange, async ((int, string) t) => await OnTabTitleChange(t));
            router.AddReactiveTarget(RxSourceEnum.TabRemove, async (int t) => await OnTabRemove(t));

            var request = Application.Current.RequestRouter();
            request.AddRequestable(RqSourceEnum.TabSetting, new TabSettingRequestable(this));

            InitializeComponent();

            InitTabView();
        }

        private async void OnTabChanged(object sender, EventArgs args)
        {
            if (CurrentPage is EmptyPage page)
            {
                await this.templateView.SetCurrentTodo(page.Setting);
            }

            Title = CurrentPage.Title;
            ChangeTab(CurrentPage);
        }

        private async void OnTabNew(string newName)
        {
            var newTab = MakeNewTab(newName);

            //表示内容設定
            await this.templateView.SetCurrentTodo(newTab.Setting);
            newTab.Content = this.templateView;
            ChangeTab(Children[0]);
        }

        private void OnTabNewOnList(string newName)
        {
            MakeNewTab(newName);
        }

        private EmptyPage MakeNewTab(string newName)
        {
            //タブIDを発行する
            var todoId = model.GetNewId();

            //タブの規定値を取得
            var setting = model.GetTabSetting(todoId);
            setting.Name.Value = newName;

            //場所を作る
            var newPosition = model.GetNewTabPosition();
            int insertIndex = 0;
            switch (newPosition)
            {
                case TabPosition.Bottom:
                    insertIndex = Children.Count();
                    break;
                case TabPosition.Left:
                    insertIndex = Children.IndexOf(tab_Origin);
                    break;
                case TabPosition.Right:
                    insertIndex = Children.IndexOf(tab_Origin) + 1;
                    break;
            }
            var newTab = new EmptyPage { Setting = setting };
            Children.Insert(insertIndex, newTab);

            return newTab;
        }

        private void OnTabJump(int todoId)
        {
            var tab = Children.OfType<EmptyPage>().Where(p => p.Setting.TodoId.Value == todoId).Select(p => p).FirstOrDefault();
            ChangeTab(tab);
        }

        private stt.Task OnTabUpDown((UpDown direction, int todoId) param)
        {
            return stt.Task.Run(() =>
            {
                model.MoveTab(param.direction, param.todoId);
            });
        }

        private stt.Task OnChangeVisibility((int todoId, bool visible) param)
        {
            return stt.Task.Run(() =>
            {
                var index = model.ChangeTabVisibility(param.todoId, param.visible);
                Children[index].IsVisible = param.visible;
            });
        }

        private stt.Task OnTabTitleChange((int todoId, string newName) param)
        {
            return stt.Task.Run(() =>
            {
                model.RenameTab(param.todoId, param.newName);
            });
        }

        private stt.Task OnTabRemove(int todoId)
        {
            return stt.Task.Run(() =>
            {
                int index = model.RemoveTab(todoId);
                Children.RemoveAt(index);
            });
        }

        private void ChangeTab(Page viewTab)
        {
            //TODO:GetTabSetting？

            if (viewTab is EmptyPage emptyPage)
            {
                tabChangeSource.Send(emptyPage.Setting);
            }
            else
            {
                tabChangeSource.Send(model.GetDefaultTabSetting());
            }

            CurrentPage = viewTab;
        }

        class TabSettingRequestable : IRequestable<TodoItem>
        {
            private TabViewPage parent;

            internal TabSettingRequestable(TabViewPage parent)
            {
                this.parent = parent;
            }

            public TodoItem Request(object param)
            {
                var todoId = (int)param;
                var setting = parent.model.GetTabSetting(todoId);
                return setting;
            }
        }
    }
}
