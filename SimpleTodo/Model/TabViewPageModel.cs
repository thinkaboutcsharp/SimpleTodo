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

        public async stt.Task<TodoItem> GetTabSetting(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }
            else
            {
                var defaultTab = await realm.GetDefaultTabSettingAsync(todoId, string.Empty);
                tabs.Add(todoId, defaultTab);
                return defaultTab;
            }
        }
    }
}
