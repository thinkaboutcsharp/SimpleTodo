using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using stt = System.Threading.Tasks;

namespace SimpleTodo
{
    public class TabViewPageModel : ModelBase
    {
        private Dictionary<int, TodoItem> tabs = new Dictionary<int, TodoItem>();

        public ObservableCollection<TodoItem> Tabs { get; private set; }
        public int SpecialTabIndex { get; private set; }
        public int LastTabIndex { get; set; }

        public TabViewPageModel(RealmAccess realm) : base(realm)
        {
            //既存のタブリストをDBから取得する
            //表示順序に注意
            Tabs = new ObservableCollection<TodoItem>(realm.SelectTodoAllAsync().Result);
        }

        public int GetNewId()
        {
            return realm.GetNewTodoId();
        }

        public TabPosition GetNewTabPosition()
        {
            return realm.GetNewTabPosition();
        }

        public async stt.Task<TodoItem> GetTabSetting(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }
            else if (todoId == CommonSettings.UndefinedId)
            {
                var defaultTab = realm.GetDefaultTabSettingAsync(todoId, string.Empty).Result;
                return defaultTab;
            }
            else
            {
                var defaultTab = await realm.GetDefaultTabSettingAsync(todoId, string.Empty);
                tabs.Add(todoId, defaultTab);
                return defaultTab;
            }
        }

        public TodoItem GetDefaultTabSetting()
        {
            return realm.GetDefaultTabSettingAsync(CommonSettings.UndefinedId, string.Empty).Result;
        }

        public void MoveTab(UpDown direction, int todoId)
        {
            var tab = Tabs.Where(t => t.TodoId.Value == todoId).First();
            var index = Tabs.IndexOf(tab);
            switch (direction)
            {
                case UpDown.Up:
                    Tabs.Move(index, index - 1);
                    break;
                case UpDown.Down:
                    Tabs.Move(index, index + 1);
                    break;
            }
        }

        public void RenameTab(int todoId, string newName)
        {
            var tab = Tabs.Where(t => t.TodoId.Value == todoId).First();
            tab.Name.Value = newName;
        }

        public int RemoveTab(int todoId)
        {
            var tab = Tabs.Where(t => t.TodoId.Value == todoId).First();
            var index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);
            return index;
        }

        public int ChangeTabVisibility(int todoId, bool visibility)
        {
            var tab = Tabs.Where(t => t.TodoId.Value == todoId).First();
            var index = Tabs.IndexOf(tab);
            tab.IsActive.Value = visibility;
            return index;
        }
    }
}
