using System;
using System.Collections.Generic;
using System.Windows.Input;
using Reactive.Bindings;
using mr = MR.Gestures;
using stt = System.Threading.Tasks;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public partial class TemplateView : ContentView, ITabPage
    {
        public TodoItem Setting { get; set; }

        private TemplateViewModel model = new TemplateViewModel(Application.Current.RealmAccess());

        private PageRotationOvserver pageRotationTarget;
        private TabListTransitObservable tabListTransitSource;
        private TabViewAppearingObserver tabViewAppearingTarget;
        private DirectTabSettingObservable directTabSettingSource;
        private TabRemoveObserver tabRemoveTarget;

        private ICommand MenuTabListCommand;
        private ICommand MenuNewTaskCommand;
        private ICommand MenuTaskUpCommand;
        private ICommand MenuTaskDownCommand;
        private ICommand MenuTabSettingCommand;

        private MenuBarItem menuTabList;
        private MenuBarItem menuNewTask;
        private MenuBarItem menuTaskUp;
        private MenuBarItem menuTaskDown;
        private MenuBarItem menuTabSetting;

        private TodoTask editingTask;

        private PageDirectionEnum currentPageDirection = PageDirectionEnum.Vertical;

        public async stt.Task SetCurrentTodo(TodoItem setting)
        {
            model.Setting = Setting = setting;
            await model.LoadTodo(setting.TodoId.Value);
            lvw_TodoList.ItemsSource = model.Todo;
        }

        public stt.Task RemoveTodo(int todoId)
        {
            return stt.Task.Run(() =>
            {
                model.RemoveTodo(todoId);
            });
        }

        public TemplateView()
        {
            InitializeComponent();

            BindingContext = model;

            model.NoTaskSelected += (_s, _e) =>
            {
                Console.WriteLine("NoTaskSelected");
            };

            MenuTabListCommand = new Command(() => OnMenuTabListTapped());
            MenuNewTaskCommand = new Command(() => OnMenuNewTaskTapped());
            MenuTaskUpCommand = new Command(async () => await model.OnTaskUp());
            MenuTaskDownCommand = new Command(async () => await model.OnTaskDown());
            MenuTabSettingCommand = new Command(() => OnMenuTabSettingTapped());

            menuTabList = new MenuBarItem { ImagePath = "icon_113430_256.png", TappedCommand = MenuTabListCommand };
            menuNewTask = new MenuBarItem { ImagePath = "icon_111600_256.jpg", TappedCommand = MenuNewTaskCommand };
            menuTaskUp = new MenuBarItem { ImagePath = "icon_112310_256.jpg", TappedCommand = MenuTaskUpCommand };
            menuTaskDown = new MenuBarItem { ImagePath = "icon_113470_256.jpg", TappedCommand = MenuTaskDownCommand };
            menuTabSetting = new MenuBarItem { ImagePath = "icon_113540_256.jpg", TappedCommand = MenuTabSettingCommand };

            SetMenuBar();

            pageRotationTarget = new PageRotationOvserver(d => OnRotation(d));
            tabListTransitSource = new TabListTransitObservable();
            tabViewAppearingTarget = new TabViewAppearingObserver(_ => SetMenuBar());
            directTabSettingSource = new DirectTabSettingObservable();
            tabRemoveTarget = new TabRemoveObserver(async t => await RemoveTodo(t));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.PageRotation, pageRotationTarget);
            router.AddReactiveSource(RxSourceEnum.TabListTransit, tabListTransitSource);
            router.AddReactiveTarget(RxSourceEnum.TabListClose, tabViewAppearingTarget);
            router.AddReactiveSource(RxSourceEnum.ClearListViewSelection, model.ClearSelectionObservable);
            router.AddReactiveSource(RxSourceEnum.DirectTabSettingMenu, directTabSettingSource);

            //上からリクエストがあればリストを取得するため、最初は何もしない
        }

        private void SetMenuBar()
        {
            var menuBar = Application.Current.MenuBarView();
            if (lay_Main.Children[0] != menuBar) lay_Main.Children.Insert(0, menuBar);

            switch (currentPageDirection)
            {
                case PageDirectionEnum.Vertical:
                    menuBar.SetValue(Grid.RowProperty, 0);
                    menuBar.SetValue(Grid.ColumnProperty, 0);
                    break;
                case PageDirectionEnum.Horizontal:
                    switch (Application.Current.CommonSettings().HorizontalMenuBarPosition)
                    {
                        case MenuBarPosition.Left:
                            menuBar.SetValue(Grid.RowProperty, 0);
                            menuBar.SetValue(Grid.ColumnProperty, 0);
                            break;
                        case MenuBarPosition.Right:
                            menuBar.SetValue(Grid.RowProperty, 0);
                            menuBar.SetValue(Grid.ColumnProperty, 1);
                            break;
                    }
                    break;
            }

            menuBar.MenuBarItem1 = menuTabList;
            menuBar.MenuBarItem2 = menuNewTask;
            menuBar.MenuBarItem3 = menuTaskUp;
            menuBar.MenuBarItem4 = menuTaskDown;
            menuBar.MenuBarItem5 = null;
            menuBar.MenuBarItemEnd = menuTabSetting;
        }

        private void OnRotation(PageDirectionEnum pageDirection)
        {
            switch (pageDirection)
            {
                case PageDirectionEnum.Horizontal:
                    RotateToHorizontal();
                    break;
                case PageDirectionEnum.Vertical:
                    RotateToVertical();
                    break;
            }
            currentPageDirection = pageDirection;
            SetMenuBar();
        }

        private void OnDelete(object sender, EventArgs args)
        {
            var item = (TodoTask)((MenuItem)sender).CommandParameter;
        }

        private void OnMenuNewTaskTapped()
        {
            ShowEditView(() => dev_TaskNameEditor.SetNewMode());
        }

        void OnEditName(object sender, EventArgs args)
        {
            var item = (TodoTask)((MenuItem)sender).CommandParameter;
            editingTask = item;

            ShowEditView(() => dev_TaskNameEditor.SetUpdateMode(item.Name.Value));
        }

        private void ShowEditView(Action SetMode)
        {
            lay_Edit.IsVisible = true;
            SetMode();

            lay_Main.IsEnabled = false;
            lay_Main.Opacity = DirectEditView.BackgroundOpacity;
        }

        private async void OnFixed(object sender, FixedEventArgs args)
        {
            if (dev_TaskNameEditor.HasName)
            {
                lay_Edit.IsVisible = false;
                lay_Main.IsEnabled = true;
                lay_Main.Opacity = 1.0;

                switch (args.EditMode)
                {
                    case DirectEditMode.New:
                        await model.AddTask(dev_TaskNameEditor.Name.Value);
                        break;
                    case DirectEditMode.Update:
                        await model.EditTask(editingTask.TaskId.Value, dev_TaskNameEditor.Name.Value);
                        break;
                }
            }

            editingTask = null;
        }

        private void OnLongTapping(object sender, mr.LongPressEventArgs args)
        {
            //LongPressingイベントはLongPressが始まるタイミングで一度だけ呼ばれる
            //LongPressedは離すまで起きない

            var currentCell = (TodoListViewCell)sender;
            currentCell.View.BackgroundColor = model.SelectingBackgroundColor;
            model.SelectOperationTask(currentCell.ItemId);
        }

        private async void OnTapped(object sender, TappedEventArgs args)
        {
            if (!model.SelectOperationTask(CommonSettings.UndefinedId))
            {
                var currentCell = (TodoListViewCell)sender;
                await model.ToggleTaskStatus(currentCell.ItemId);
            }
        }

        private void OnDown(object sender, mr.DownUpEventArgs args)
        {
            model.ClearSelection();
        }

        private void RotateToVertical()
        {
            var common = Application.Current.CommonSettings();
            lay_Main.RowDefinitions[0].Height = 40;
            lay_Main.RowDefinitions[1].Height = GridLength.Auto;
            lay_Main.ColumnDefinitions[0].Width = GridLength.Star;
            lay_Main.ColumnDefinitions[1].Width = 0;

            var menuBar = Application.Current.MenuBarView();
            menuBar.SetValue(Grid.RowProperty, 0);
            menuBar.SetValue(Grid.ColumnProperty, 0);
            lvw_TodoList.SetValue(Grid.RowProperty, 1);
            lvw_TodoList.SetValue(Grid.ColumnProperty, 0);

            lay_Main.Margin = new Thickness(10, 0, 5, 0);
        }

        private void RotateToHorizontal()
        {
            var common = Application.Current.CommonSettings();
            var menuBar = Application.Current.MenuBarView();

            if (common.HorizontalMenuBarPosition == MenuBarPosition.Left)
            {
                lay_Main.RowDefinitions[0].Height = GridLength.Auto;
                lay_Main.RowDefinitions[1].Height = 0;
                lay_Main.ColumnDefinitions[0].Width = 45;
                lay_Main.ColumnDefinitions[1].Width = GridLength.Star;

                menuBar.SetValue(Grid.RowProperty, 0);
                menuBar.SetValue(Grid.ColumnProperty, 0);
                lvw_TodoList.SetValue(Grid.RowProperty, 0);
                lvw_TodoList.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                lay_Main.RowDefinitions[0].Height = GridLength.Auto;
                lay_Main.RowDefinitions[1].Height = 0;
                lay_Main.ColumnDefinitions[0].Width = GridLength.Star;
                lay_Main.ColumnDefinitions[1].Width = 45;

                menuBar.SetValue(Grid.RowProperty, 0);
                menuBar.SetValue(Grid.ColumnProperty, 1);
                lvw_TodoList.SetValue(Grid.RowProperty, 0);
                lvw_TodoList.SetValue(Grid.ColumnProperty, 0);
            }

            lay_Main.Margin = new Thickness(40, 0, 40, 0);
        }

        private void OnMenuTabListTapped()
        {
            tabListTransitSource.Send(null);
        }

        private void OnMenuTabSettingTapped()
        {
            directTabSettingSource.Send(Setting.TodoId.Value);
        }
    }
}
