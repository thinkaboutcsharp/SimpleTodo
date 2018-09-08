using System;
using System.Collections.Generic;
using stt = System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class TabViewPage : TabbedPage
    {
        private TodoTabNewObserver tabNewTarget;
        private TabJumpingObserver tabJumpingTarget;
        private CentralViewChangeObservable tabChangeSource;
        private TabUpDownObserver tabUpDownTarget;
        private ChangeVisibilityObserver changeVisibilityTarget;
        private TabTitleChangeObserver tabTitleChangeTarget;
        private TabRemoveObserver tabRemoveTarget;

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

            tabNewTarget = new TodoTabNewObserver(async p => await OnTabNew(p));
            tabJumpingTarget = new TabJumpingObserver(t => OnTabJump(t));
            tabChangeSource = new CentralViewChangeObservable();
            tabUpDownTarget = new TabUpDownObserver(async p => await OnTabUpDown(p));
            changeVisibilityTarget = new ChangeVisibilityObserver(async p => await OnChangeVisibility(p));
            tabTitleChangeTarget = new TabTitleChangeObserver(async p => await OnTabTitleChange(p));
            tabRemoveTarget = new TabRemoveObserver(async t => await OnTabRemove(t));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.TodoTabNew, tabNewTarget);
            router.AddReactiveTarget(RxSourceEnum.TabJumping, tabJumpingTarget);
            router.AddReactiveSource(RxSourceEnum.CentralViewChange, tabChangeSource);
            router.AddReactiveTarget(RxSourceEnum.TabUpDown, tabUpDownTarget);
            router.AddReactiveTarget(RxSourceEnum.TodoTabVisibleChange, changeVisibilityTarget);
            router.AddReactiveTarget(RxSourceEnum.TabTitleChange, tabTitleChangeTarget);
            router.AddReactiveTarget(RxSourceEnum.TabRemove, tabRemoveTarget);
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

        private async stt.Task OnTabNew(string newName)
        {
            var newTab = await MakeNewTab(newName);

            //表示内容設定
            await this.templateView.SetCurrentTodo(newTab.Setting);
            newTab.Content = this.templateView;
            ChangeTab(Children[0]);
        }

        private stt.Task OnTabNewOnList(string newName)
        {
            return stt.Task.Run(async () =>
            {
                await MakeNewTab(newName);
            });
        }

        private async stt.Task<EmptyPage> MakeNewTab(string newName)
        {
            //タブIDを発行する
            var todoId = model.GetNewId();

            //タブの規定値を取得
            var setting = await model.GetTabSetting(todoId);
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
    }
}
