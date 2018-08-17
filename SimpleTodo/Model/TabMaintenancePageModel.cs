using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class TabMaintenancePageModel
    {
        public const int UndefinedTodoId = -1;

        public event EventHandler NoTaskSelected;

        public ReactiveProperty<Color> NormalBackgroundColor { get; }
        public ReactiveProperty<Color> SelectingBackgroundColor { get; }

        public IObservable<Color> ClearSelectionSource { get => clearSelectionSource; }
        public IObservable<(int, bool)> ChangeVisibilitySource { get => changeVisibilitySource; }
        public IObservable<(TabUpDown, int)> TabUpDownSource { get => tabUpDownSource; }

        public ObservableCollection<TodoItem> TodoList { get; private set; }

        private List<TodoItem> todos = new List<TodoItem>();

        private int selectingTodoId;

        private ClearSelectionObservable clearSelectionSource = new ClearSelectionObservable();
        private ChangeVisibilityObservable changeVisibilitySource = new ChangeVisibilityObservable();
        private TabUpDownObservable tabUpDownSource = new TabUpDownObservable();

        public TabMaintenancePageModel()
        {
            selectingTodoId = UndefinedTodoId;

            //DB
            NormalBackgroundColor = new ReactiveProperty<Color>(Color.Transparent);
            SelectingBackgroundColor = new ReactiveProperty<Color>(Color.Red);

            todos.Add(new TodoItem(0, "アイウエオ"));
            todos.Add(new TodoItem(1, "カキクケコ"));

            TodoList = new ObservableCollection<TodoItem>(todos);
        }

        public bool SelectOperationTodo(int todoId)
        {
            var hadId = selectingTodoId == UndefinedTodoId ? false : true;
            selectingTodoId = todoId;
            return hadId;
        }

        public void AddTodoTab(string name)
        {
            todos.Add(new TodoItem(GetNewId(), name));
        }

        public void EditTodo(int todoId, string name)
        {
            var current = todos.Select(todoId);
            if (current == null) return;
            current.Name.Value = name;
        }

        public void ClearSelection()
        {
            clearSelectionSource.Send(NormalBackgroundColor.Value);
        }

        public void ChangeVisibility(int todoId)
        {
            var todo = todos.Select(todoId);
            todo.IsActive.Value = !todo.IsActive.Value;
            changeVisibilitySource.Send((todo.TodoId.Value, todo.IsActive.Value));
        }

        private int GetNewId()
        {
            return todos.Count;
        }

        public void OnTodoUp()
        {
            if (selectingTodoId == UndefinedTodoId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var todo = todos.Select(selectingTodoId);
            var index = TodoList.IndexOf(todo);
            if (index > 0)
            {
                TodoList.Move(index, index - 1);
                tabUpDownSource.Send((TabUpDown.Up, todo.TodoId.Value));
            }
        }

        public void OnTodoDown()
        {
            if (selectingTodoId == UndefinedTodoId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var todo = todos.Select(selectingTodoId);
            var index = TodoList.IndexOf(todo);
            if (index < todos.Count - 1)
            {
                TodoList.Move(index, index + 1);
                tabUpDownSource.Send((TabUpDown.Down, todo.TodoId.Value));
            }
        }
    }

    static class TodoExtension
    {
        public static TodoItem Select(this List<TodoItem> tasks, int id)
        {
            return tasks.Where(t => t.TodoId.Value == id).Select(t => t).FirstOrDefault();
        }
    }
}
