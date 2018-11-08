using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using stt = System.Threading.Tasks;
using Reactive.Bindings;
using Anywhere;

namespace SimpleTodo
{
    public class TabMaintenancePageModel : ModelBase
    {
        public event EventHandler NoTaskSelected;

        public ReactiveProperty<Color> NormalBackgroundColor { get; }
        public ReactiveProperty<Color> SelectingBackgroundColor { get; }

        public IReactiveSource<ListType> ClearSelectionSource { set => clearSelectionSource = value; }
        public IReactiveSource<(int, bool)> ChangeVisibilitySource { set => changeVisibilitySource = value; }
        public IReactiveSource<(UpDown, int)> TabUpDownSource { set => tabUpDownSource = value; }

        public ObservableCollection<TodoItem> TodoList { get; private set; }

        private int selectingTodoId;

        private IReactiveSource<ListType> clearSelectionSource;
        private IReactiveSource<(int, bool)> changeVisibilitySource;
        private IReactiveSource<(UpDown, int)> tabUpDownSource;

        public TabMaintenancePageModel(IDataAccess realm) : base(realm)
        {
            selectingTodoId = CommonSettings.UndefinedId;

            var colorPattern = realm.GetDefaultColorPattern();
            NormalBackgroundColor = new ReactiveProperty<Color>(colorPattern.TabListViewCellColor);
            SelectingBackgroundColor = new ReactiveProperty<Color>(colorPattern.TabListViewCellSelectedColor);

            TodoList = new ObservableCollection<TodoItem>(realm.SelectTodoAllAsync().Result);
        }

        public string GetMenuBarIcon(MenuBarIcon barIcon)
        {
            return dataAccess.GetMenuBarIconFile(barIcon);
        }

        public bool SelectOperationTodo(int todoId)
        {
            var hadId = selectingTodoId == CommonSettings.UndefinedId ? false : true;
            selectingTodoId = todoId;
            return hadId;
        }

        public int GetSelectingId() => selectingTodoId;

        public void AddTodoTab(string name)
        {
            var newTodo = new TodoItem(dataAccess.GetNewTodoId(), name, 0, true);
            newTodo.IconPattern = dataAccess.GetDefaultIconPattern();
            newTodo.ColorPattern = dataAccess.GetDefaultColorPattern();
            newTodo.IconPatternId = newTodo.IconPattern.IconId;
            newTodo.ColorPatternId = newTodo.ColorPattern.ColorId;

            TodoList.Insert(0, newTodo);

            dataAccess.AddTodoAsync(newTodo);
            dataAccess.ReorderTodoAsync(TodoList);
        }

        public void EditTodo(int todoId, string name)
        {
            var current = TodoList.Select(todoId);
            if (current == null) return;
            current.Name.Value = name;

            dataAccess.RenameTodoAsync(todoId, name);
        }

        public void ClearSelection()
        {
            selectingTodoId = CommonSettings.UndefinedId;
            clearSelectionSource.Send(ListType.Task);
        }

        public void ChangeVisibilityAsync(int todoId)
        {
            var todo = TodoList.Select(todoId);
            todo.IsActive.Value = !todo.IsActive.Value;
            changeVisibilitySource.Send((todo.TodoId.Value, todo.IsActive.Value));

            dataAccess.ChangeVisibilityAsync(todoId, todo.IsActive.Value);
        }

        public void OnTodoUp()
        {
            if (selectingTodoId == CommonSettings.UndefinedId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var todo = TodoList.Select(selectingTodoId);
            var index = TodoList.IndexOf(todo);
            if (index > 0)
            {
                TodoList.Move(index, index - 1);
                tabUpDownSource.Send((UpDown.Up, todo.TodoId.Value));

                dataAccess.ReorderTodoAsync(TodoList);
            }
        }

        public void OnTodoDown()
        {
            if (selectingTodoId == CommonSettings.UndefinedId)
            {
                NoTaskSelected?.Invoke(this, new EventArgs());
                return;
            }

            var todo = TodoList.Select(selectingTodoId);
            var index = TodoList.IndexOf(todo);
            if (index < TodoList.Count - 1)
            {
                TodoList.Move(index, index + 1);
                tabUpDownSource.Send((UpDown.Down, todo.TodoId.Value));

                dataAccess.ReorderTodoAsync(TodoList);
            }
        }
    }

    static class TodoExtension
    {
        public static TodoItem Select(this ObservableCollection<TodoItem> todos, int id)
        {
            return todos.Where(t => t.TodoId.Value == id).FirstOrDefault();
        }
    }
}
