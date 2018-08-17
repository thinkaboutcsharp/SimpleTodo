using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Drawing;

namespace SimpleTodo
{
    public class TemplateViewModel
    {
        public const int UndefinedTaskId = -1;

        public ObservableCollection<TodoTask> Todo { get; private set; }

        public Color NormalBackgroundColor { get => Setting.ColorPattern.TodoViewCellColor; }
        public Color SelectingBackgroundColor { get => Setting.ColorPattern.TodoViewCellSelectedColor; }

        public IObservable<Color> ClearSelectionObservable { get => clearSelectionSource; }

        public event EventHandler NoTaskSelected;
        public TodoItem Setting { get; set; }

        private Dictionary<int, List<TodoTask>> tabs = new Dictionary<int, List<TodoTask>>();

        private List<TodoTask> viewingTodo;
        private int selectionTaskId;

        private ClearSelectionObservable clearSelectionSource = new ClearSelectionObservable();

        public void LoadTodo(int todoId)
        {
            selectionTaskId = UndefinedTaskId;

            if (tabs.ContainsKey(todoId))
            {
                viewingTodo = tabs[todoId];
                Todo = new ObservableCollection<TodoTask>(viewingTodo);
                return;
            }

            var todo = new List<TodoTask>();
            SelectTodo(todo, todoId);
            tabs.Add(todoId, todo);
            viewingTodo = todo;
            Todo = new ObservableCollection<TodoTask>(viewingTodo);
        }

        public void ToggleTaskStatus(int taskId)
        {
            var task = viewingTodo.Select(taskId);
            //暫定
            switch (task.Status.Value)
            {
                case TaskStatus.Unchecked:
                    task.Status.Value = TaskStatus.Checked;
                    break;
                case TaskStatus.Checked:
                    task.Status.Value = TaskStatus.Unchecked;
                    break;
            }
        }

        public bool SelectOperationTask(int taskId)
        {
            var hadId = selectionTaskId == UndefinedTaskId ? false : true;
            selectionTaskId = taskId;
            return hadId;
        }

        public void ClearSelection()
        {
            clearSelectionSource.Send(Setting.ColorPattern.PageBasicBackgroundColor);
        }

        public void AddTask(string taskName)
        {
            viewingTodo.Insert(0, new TodoTask(GetNewId(), taskName, TaskStatus.Unchecked, 0));
            Todo.Insert(0, viewingTodo[0]);
        }

        public void EditTask(int taskId, string taskName)
        {
            var task = viewingTodo.Select(taskId);
            task.Name.Value = taskName;
        }

        public void OnTaskUp()
        {
            if (selectionTaskId == UndefinedTaskId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var task = viewingTodo.Select(selectionTaskId);
            var index = Todo.IndexOf(task);
            if (index > 0)
            {
                Todo.Move(index, index - 1);
            }
        }

        public void OnTaskDown()
        {
            if (selectionTaskId == UndefinedTaskId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var task = viewingTodo.Select(selectionTaskId);
            var index = Todo.IndexOf(task);
            if (index < viewingTodo.Count - 1)
            {
                Todo.Move(index, index + 1);
            }
        }

        private void SelectTodo(List<TodoTask> result, int todoId)
        {
        }

        private int GetNewId()
        {
            return viewingTodo.Count;
        }
    }

    static class TaskExtension
    {
        public static TodoTask Select(this List<TodoTask> tasks, int id)
        {
            return tasks.Where(t => t.TaskId.Value == id).Select(t => t).FirstOrDefault();
        }
    }
}
