using System;
using System.Drawing;
using System.Collections.Generic;

namespace SimpleTodo
{
    public class TabViewPageModel
    {
        private Dictionary<int, Tab> tabs = new Dictionary<int, Tab>();

        public TabViewPageModel()
        {
            //既存のタブリストを取得する
        }

        public int GetNewId()
        {
            return tabs.Count; //ちゃんとDBで管理する
        }

        public Tab GetTabSetting(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }
            else
            {
                var newTab = new Tab
                {
                    TodoId = todoId,
                    Name = string.Empty,
                    ColorPattern = new ColorSetting //DBから取る
                    {
                        PageBackground = Color.Blue
                    }
                };
                tabs.Add(todoId, newTab);
                return newTab;
            }
        }
    }

    public class Tab
    {
        public int TodoId { get; set; }
        public string Name { get; set; }
        public ColorSetting ColorPattern { get; set; }
    }

    public class ColorSetting
    {
        public Color PageBackground { get; set; }
    }
}
