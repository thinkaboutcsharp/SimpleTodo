using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Drawing;
using stt = System.Threading.Tasks;
using Anywhere;

namespace SimpleTodo
{
    public class TemplateViewModel : ModelBase
    {
        class SuspendTarget : ObserverBase<Suspend>
        {
            internal SuspendTarget(TemplateViewModel parent) : base((s) => {
                if (s == Suspend.Sleep) parent.ClearTodo();
            })
            {}
        }

        public ObservableCollection<TodoTask> Todo { get; private set; }

        public IReactiveSource<ListType> ClearSelectionObservable { set => clearSelectionSource = value; }
        public IObserver<Suspend> SuspendObserver { get; }

        public event EventHandler NoTaskSelected;
        public TodoItem Setting { get; set; }

        private Dictionary<int, ObservableCollection<TodoTask>> tabs = new Dictionary<int, ObservableCollection<TodoTask>>();

        private int selectionTaskId;

        private IReactiveSource<ListType> clearSelectionSource;

        public TemplateViewModel(IDataAccess realm) : base(realm)
        {
            SuspendObserver = new SuspendTarget(this);
        }

        public async stt.Task LoadTodo(int todoId)
        {
            selectionTaskId = CommonSettings.UndefinedId;

            if (tabs.ContainsKey(todoId))
            {
                Todo = tabs[todoId];
                return;
            }

            var newTodo = new ObservableCollection<TodoTask>(await dataAccess.SelectTaskAllAsync(todoId));
            tabs.Add(todoId, newTodo);
            Todo = newTodo;
        }

        public void RemoveTodo(int todoId)
        {
            tabs.Remove(todoId);
        }

        void ClearTodo()
        {
            tabs.Clear();
        }

        public void ToggleTaskStatus(int taskId)
        {
            var task = Todo.Select(taskId);

            switch (task.Status.Value)
            {
                case TaskStatus.Unchecked:
                    task.Status.Value = TaskStatus.Checked;
                    break;
                case TaskStatus.Checked:
                    task.Status.Value = Setting.UseTristate.Value ? TaskStatus.Checked : TaskStatus.Unchecked;
                    break;
                case TaskStatus.Canceled:
                    task.Status.Value = TaskStatus.Unchecked;
                    break;
            }

            dataAccess.ToggleTaskStatusAsync(Setting.TodoId.Value, taskId, task.Status.Value);
        }

        public bool SelectOperationTask(int taskId)
        {
            var hadId = selectionTaskId == CommonSettings.UndefinedId ? false : true;
            selectionTaskId = taskId;
            return hadId;
        }

        public void ClearSelection()
        {
            clearSelectionSource.Send(ListType.Todo);
        }

        public void AddTask(string taskName)
        {
            var newTask = new TodoTask(dataAccess.GetNewTaskId(Setting.TodoId.Value), taskName, TaskStatus.Unchecked, 0);
            Todo.Insert(0, newTask);
            dataAccess.AddTaskAsync(Setting.TodoId.Value, newTask);
            dataAccess.ReorderTaskAsync(Setting.TodoId.Value, Todo);
        }

        public void EditTask(int taskId, string taskName)
        {
            var task = Todo.Select(taskId);
            task.Name.Value = taskName;
            dataAccess.RenameTaskAsync(Setting.TodoId.Value, taskId, taskName);
        }

        public void OnTaskUp()
        {
            if (selectionTaskId == CommonSettings.UndefinedId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var task = Todo.Select(selectionTaskId);
            var index = Todo.IndexOf(task);
            if (index > 0)
            {
                Todo.Move(index, index - 1);
            }

            dataAccess.ReorderTaskAsync(Setting.TodoId.Value, Todo);
        }

        public void OnTaskDown()
        {
            if (selectionTaskId == CommonSettings.UndefinedId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var task = Todo.Select(selectionTaskId);
            var index = Todo.IndexOf(task);
            if (index < Todo.Count - 1)
            {
                Todo.Move(index, index + 1);
            }

            dataAccess.ReorderTaskAsync(Setting.TodoId.Value, Todo);
        }
    }

    static class TaskExtension
    {
        public static TodoTask Select(this ObservableCollection<TodoTask> tasks, int id)
        {
            return tasks.Where(t => t.TaskId.Value == id).FirstOrDefault();
        }
    }
}
