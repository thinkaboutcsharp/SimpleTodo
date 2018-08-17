using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTodo
{
    public class TabViewPageModel
    {
        private Dictionary<int, TodoItem> tabs = new Dictionary<int, TodoItem>();

        public IEnumerable<TodoItem> Tabs { get; private set; }
        public int SpecialTabIndex { get; private set; }
        public int LastTabIndex { get; set; }

        public TabViewPageModel()
        {
            //既存のタブリストをDBから取得する
            //表示順序に注意

        }

        public int GetNewId()
        {
            return tabs.Count; //ちゃんとDBで管理する
        }

        public TodoItem GetTabSetting(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }
            else
            {
                var newTab = new TodoItem
                {
                };
                tabs.Add(todoId, newTab);
                return newTab;
            }
        }
    }
}
