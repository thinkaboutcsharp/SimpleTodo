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

        public TabViewPageModel(IDataAccess dataAccess) : base(dataAccess)
        {
            Tabs = new ObservableCollection<TodoItem>(dataAccess.SelectTodoAllAsync().Result);
        }

        public int GetNewId()
        {
            return dataAccess.GetNewTodoId();
        }

        public TabPosition GetNewTabPosition()
        {
            return dataAccess.GetNewTabPosition();
        }

        public TodoItem GetTabSetting(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }
            else if (todoId == CommonSettings.UndefinedId)
            {
                var defaultTab = dataAccess.GetDefaultTabSetting(todoId, string.Empty);
                return defaultTab;
            }
            else
            {
                var defaultTab = dataAccess.GetDefaultTabSetting(todoId, string.Empty);
                tabs.Add(todoId, defaultTab);
                return defaultTab;
            }
        }

        public void AddTab(TodoItem todo)
        {
            dataAccess.AddTodoAsync(todo);

            Tabs.Insert(todo.DisplayOrder.Value, todo);
            dataAccess.ReorderTodoAsync(Tabs);
        }

        public TodoItem GetDefaultTabSetting()
        {
            return dataAccess.GetDefaultTabSetting(CommonSettings.UndefinedId, string.Empty);
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
            dataAccess.ReorderTodoAsync(Tabs);
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
