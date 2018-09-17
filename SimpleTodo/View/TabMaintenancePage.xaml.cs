using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.ComponentModel;
using mr = MR.Gestures;
using Reactive.Bindings;

using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class TabMaintenancePage : global::Xamarin.Forms.ContentPage
    {
        private TabMaintenancePageModel model = new TabMaintenancePageModel(Application.Current.DataAccess());

        private ICommand MenuNewTabCommand;
        private ICommand MenuTabUpCommand;
        private ICommand MenuTabDownCommand;
        private ICommand MenuVisibleSwitchOnOffCommand;
        private ICommand MenuTabSettingCommand;

        private MenuBarItem menuNewTab;
        private MenuBarItem menuTabUp;
        private MenuBarItem menuTabDown;
        private MenuBarItem menuSwitchOnOff;
        private MenuBarItem menuTabSetting;

        public ICommand VisibleChangedCommand { get; set; }

        private TabMaintenanceDisppearingObservable tabMaintenanceDisappearingSource = new TabMaintenanceDisppearingObservable();
        private TabJumpingObservable tabJumpingSource = new TabJumpingObservable();
        private VisibleSwitchOnOffObservable visibleSwitchOnOffSource = new VisibleSwitchOnOffObservable();
        private TabNewOnListObservable tabNewOnListSource = new TabNewOnListObservable();
        private TabTitleChangeObservable titleChangeSource = new TabTitleChangeObservable();
        private TabRemoveObservable tabRemoveSource = new TabRemoveObservable();
        private DirectTabSettingObservable directTabSettingSource = new DirectTabSettingObservable();

        private TodoItem editingItem; //編集対象のアイテムを記憶する（セルが拾えないから）

        private int ShownSwitchCount;

        public TabMaintenancePage()
        {
            InitializeComponent();

            ShownSwitchCount = 0;

            MenuNewTabCommand = new Command(() => OnMenuNewTabTapped());
            MenuTabUpCommand = new Command(async () => await model.OnTodoUp());
            MenuTabDownCommand = new Command(async () => await model.OnTodoDown());
            MenuVisibleSwitchOnOffCommand = new Command(() => OnMenuVisibleSwitchOnOff());
            MenuTabSettingCommand = new Command(() => OnMenuTabSetting());

            menuNewTab = new MenuBarItem { ImagePath = model.GetMenuBarIcon(MenuBarIcon.NewTab), TappedCommand = MenuNewTabCommand };
            menuTabUp = new MenuBarItem { ImagePath = model.GetMenuBarIcon(MenuBarIcon.Up), TappedCommand = MenuTabUpCommand };
            menuTabDown = new MenuBarItem { ImagePath = model.GetMenuBarIcon(MenuBarIcon.Down), TappedCommand = MenuTabDownCommand };
            menuSwitchOnOff = new MenuBarItem { ImagePath = model.GetMenuBarIcon(MenuBarIcon.SwitchOnOff), TappedCommand = MenuVisibleSwitchOnOffCommand };
            menuTabSetting = new MenuBarItem { ImagePath = model.GetMenuBarIcon(MenuBarIcon.TabSetting), TappedCommand = MenuTabSettingCommand };

            SetMenuBar();

            VisibleChangedCommand = new Command<int>(async Id => await model.ChangeVisibilityAsync(Id));

            this.Appearing += (_s, _e) => SetMenuBar();
            this.Disappearing += (_s, _e) => tabMaintenanceDisappearingSource.Send(null);

            var router = Application.Current.ReactionRouter();
            router.AddReactiveSource(RxSourceEnum.ClearListViewSelection, model.ClearSelectionSource);
            router.AddReactiveSource(RxSourceEnum.TabListClose, tabMaintenanceDisappearingSource);
            router.AddReactiveSource(RxSourceEnum.TabJumping, tabJumpingSource);
            router.AddReactiveSource(RxSourceEnum.TodoTabVisibleChange, model.ChangeVisibilitySource);
            router.AddReactiveSource(RxSourceEnum.VisibleSwitchOnOff, visibleSwitchOnOffSource);
            router.AddReactiveSource(RxSourceEnum.TabUpDown, model.TabUpDownSource);
            router.AddReactiveSource(RxSourceEnum.TodoTabNewOnList, tabNewOnListSource);
            router.AddReactiveSource(RxSourceEnum.TabTitleChange, titleChangeSource);
            router.AddReactiveSource(RxSourceEnum.TabRemove, tabRemoveSource);
            router.AddReactiveSource(RxSourceEnum.DirectTabSettingMenu, directTabSettingSource);

            BindingContext = this;
            lvw_TabMaintenance.ItemsSource = model.TodoList;
        }

        void OnLongTapped(object sender, mr.LongPressEventArgs args)
        {
            var currentCell = (TodoListViewCell)sender;
            model.SelectOperationTodo(currentCell.ItemId);
            currentCell.View.BackgroundColor = model.SelectingBackgroundColor.Value;
        }

        void OnTapped(object sender, TappedEventArgs args)
        {
            if (!model.SelectOperationTodo(CommonSettings.UndefinedId))
            {
                //Switchを表示
                var viewCell = (TodoListViewCell)sender;
                viewCell.IsSelected.Value = !viewCell.IsSelected.Value;

                if (viewCell.IsSelected.Value) ShownSwitchCount++;
                else ShownSwitchCount--;
            }
        }

        void OnDoubleTapped(object sender, mr.TapEventArgs args)
        {
            tabJumpingSource.Send(((TodoListViewCell)sender).ItemId);
        }

        private void OnDown(object sender, mr.DownUpEventArgs args)
        {
            model.ClearSelection();
        }

        void OnDelete(object sender, EventArgs args)
        {
            var item = (TodoItem)((MenuItem)sender).CommandParameter;
            //TODO:DBから削除

            tabRemoveSource.Send(item.TodoId.Value);
        }

        private void OnMenuNewTabTapped()
        {
            ShowEditView(() => dev_NameEditor.SetNewMode());
        }

        void OnEditName(object sender, EventArgs args)
        {
            var menu = (MenuItem)sender;
            var item = (TodoItem)menu.CommandParameter;
            editingItem = item;

            ShowEditView(() => dev_NameEditor.SetUpdateMode(item.Name.Value));
        }

        private void OnMenuVisibleSwitchOnOff()
        {
            visibleSwitchOnOffSource.Send(ShownSwitchCount == 0 ? true : false);

            if (ShownSwitchCount == 0)
            {
                ShownSwitchCount = model.TodoList.Count;
            }
            else
            {
                ShownSwitchCount = 0;
            }
        }

        private void OnMenuTabSetting()
        {
            directTabSettingSource.Send(model.GetSelectingId());
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
            if (dev_NameEditor.HasName)
            {
                int todoId;
                string name;
                switch (args.EditMode)
                {
                    case DirectEditMode.New:
                        name = dev_NameEditor.Name.Value;
                        await model.AddTodoTab(name);
                        tabNewOnListSource.Send(name);
                        break;
                    case DirectEditMode.Update:
                        todoId = editingItem.TodoId.Value;
                        name = dev_NameEditor.Name.Value;
                        await model.EditTodo(todoId, name);
                        titleChangeSource.Send((todoId, name));
                        break;
                }
            }

            lay_Edit.IsVisible = false;
            lay_Main.IsEnabled = true;
            lay_Main.Opacity = 1.0;

            editingItem = null;
        }

        private void SetMenuBar()
        {
            var menuBar = Application.Current.MenuBarView();
            if (lay_Main.Children[0] != menuBar) lay_Main.Children.Insert(0, menuBar);

            menuBar.MenuBarItem1 = null;
            menuBar.MenuBarItem2 = menuNewTab;
            menuBar.MenuBarItem3 = menuTabUp;
            menuBar.MenuBarItem4 = menuTabDown;
            menuBar.MenuBarItem5 = menuSwitchOnOff;
            menuBar.MenuBarItemEnd = menuTabSetting;
        }
    }

    static class TodoListViewCellEx //なんとかならんのか
    {
        public static Label LabelCell(this TodoListViewCell cell)
        {
            return (Label)((StackLayout)cell.View).Children[0];
        }
        public static Switch SwitchCell(this TodoListViewCell cell)
        {
            return (Switch)((StackLayout)cell.View).Children[1];
        }
    }
}
