using System;
using System.Collections.Generic;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class TemplateViewModel
    {
        private Dictionary<int, List<TodoTask>> tabs = new Dictionary<int, List<TodoTask>>();

        public IList<TodoTask> LoadTodo(int todoId)
        {
            if (tabs.ContainsKey(todoId))
            {
                return tabs[todoId];
            }

            var todos = new List<TodoTask>();
            SelectTodo(todos, todoId);
            tabs.Add(todoId, todos);
            return todos;
        }

        private void SelectTodo(List<TodoTask> result, int todoId)
        {
            result.Add(new TodoTask(1, "あいうえお" + todoId, false));
            result.Add(new TodoTask(2, "かきくけこ" + todoId, false));
        }

        public class TodoTask
        {
            public ReactiveProperty<int> TaskId { get; private set; }
            public ReactiveProperty<string> Task { get; private set; }
            public ReactiveProperty<bool> Checked { get; private set; }

            public TodoTask(int taskId, string task, bool isChecked)
            {
                TaskId = new ReactiveProperty<int>(taskId);
                Task = new ReactiveProperty<string>(task);
                Checked = new ReactiveProperty<bool>(isChecked);
            }
        }
    }
}
