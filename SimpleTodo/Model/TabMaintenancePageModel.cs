using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using stt = System.Threading.Tasks;
using Reactive.Bindings;

namespace SimpleTodo
{
    public class TabMaintenancePageModel : ModelBase
    {
        public event EventHandler NoTaskSelected;

        public ReactiveProperty<Color> NormalBackgroundColor { get; }
        public ReactiveProperty<Color> SelectingBackgroundColor { get; }

        public IObservable<Color> ClearSelectionSource { get => clearSelectionSource; }
        public IObservable<(int, bool)> ChangeVisibilitySource { get => changeVisibilitySource; }
        public IObservable<(UpDown, int)> TabUpDownSource { get => tabUpDownSource; }

        public ObservableCollection<TodoItem> TodoList { get; private set; }

        private int selectingTodoId;

        private ClearSelectionObservable clearSelectionSource = new ClearSelectionObservable();
        private ChangeVisibilityObservable changeVisibilitySource = new ChangeVisibilityObservable();
        private TabUpDownObservable tabUpDownSource = new TabUpDownObservable();

        public TabMaintenancePageModel(RealmAccess realm) : base(realm)
        {
            selectingTodoId = CommonSettings.UndefinedId;

            var colorPattern = realm.GetDefaultColorPatternAsync().Result;
            NormalBackgroundColor.Value = colorPattern.TabListViewCellColor;
            SelectingBackgroundColor.Value = colorPattern.TabListViewCellSelectedColor;

            TodoList = new ObservableCollection<TodoItem>(realm.SelectTodoAllAsync().Result);
        }

        public string GetMenuBarIcon(MenuBarIcon barIcon)
        {
            return realm.GetMenuBarIconFile(barIcon);
        }

        public bool SelectOperationTodo(int todoId)
        {
            var hadId = selectingTodoId == CommonSettings.UndefinedId ? false : true;
            selectingTodoId = todoId;
            return hadId;
        }

        public int GetSelectingId() => selectingTodoId;

        public async stt.Task AddTodoTab(string name)
        {
            var newTodo = new TodoItem(realm.GetNewTodoId(), name, 0, true);
            newTodo.IconPattern = await realm.GetDefaultIconPatternAsync();
            newTodo.ColorPattern = await realm.GetDefaultColorPatternAsync();
            newTodo.IconPatternId = newTodo.IconPattern.IconId;
            newTodo.ColorPatternId = newTodo.ColorPattern.ColorId;

            TodoList.Insert(0, newTodo);

            await realm.AddTodoAsync(newTodo);
            await realm.ReorderTodoAsync(TodoList);
        }

        public async stt.Task EditTodo(int todoId, string name)
        {
            var current = TodoList.Select(todoId);
            if (current == null) return;
            current.Name.Value = name;

            await realm.RenameTodoAsync(todoId, name);
        }

        public void ClearSelection()
        {
            selectingTodoId = CommonSettings.UndefinedId;
            clearSelectionSource.Send(NormalBackgroundColor.Value);
        }

        public async stt.Task ChangeVisibilityAsync(int todoId)
        {
            var todo = TodoList.Select(todoId);
            todo.IsActive.Value = !todo.IsActive.Value;
            changeVisibilitySource.Send((todo.TodoId.Value, todo.IsActive.Value));

            await realm.ChangeVisibilityAsync(todoId, todo.IsActive.Value);
        }

        public async stt.Task OnTodoUp()
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

                await realm.ReorderTodoAsync(TodoList);
            }
        }

        public async stt.Task OnTodoDown()
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

                await realm.ReorderTodoAsync(TodoList);
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
