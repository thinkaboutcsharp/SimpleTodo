using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using stt = System.Threading.Tasks;

using PCLStorage;
using SQLite;
using AutoMapper;

namespace SimpleTodo.SQLite
{
    public class SQLiteAccess : IDataAccess
    {
        public stt.Task<TodoTask> AddTaskAsync(int todoId, TodoTask todoTask)
        {
            throw new NotImplementedException();
        }

        public stt.Task<TodoItem> AddTodoAsync(TodoItem todoItem)
        {
            throw new NotImplementedException();
        }

        public stt.Task BeginFromTabListAsync(bool begin)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public stt.Task ChangeVisibilityAsync(int todoId, bool visibile)
        {
            throw new NotImplementedException();
        }

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public stt.Task DeleteTaskAsync(int todoId, int taskId)
        {
            throw new NotImplementedException();
        }

        public stt.Task DeleteTodoAsync(int todoId)
        {
            throw new NotImplementedException();
        }

        public stt.Task<IReadOnlyList<ColorSetting>> GetColorPatternAllAsync()
        {
            throw new NotImplementedException();
        }

        public stt.Task<ColorSetting> GetDefaultColorPatternAsync()
        {
            throw new NotImplementedException();
        }

        public stt.Task<IconSetting> GetDefaultIconPatternAsync()
        {
            throw new NotImplementedException();
        }

        public stt.Task<TodoItem> GetDefaultTabSettingAsync(int newTodoId, string newName)
        {
            throw new NotImplementedException();
        }

        public TaskOrderPattern GetDefaultTaskOrder()
        {
            throw new NotImplementedException();
        }

        public bool GetDefaultUseTristate()
        {
            throw new NotImplementedException();
        }

        public stt.Task<IReadOnlyList<IconSetting>> GetIconPatternAllAsync()
        {
            throw new NotImplementedException();
        }

        public ViewingPage GetLastPage()
        {
            throw new NotImplementedException();
        }

        public int GetLastTabIndex()
        {
            throw new NotImplementedException();
        }

        public string GetMenuBarIconFile(MenuBarIcon menu)
        {
            throw new NotImplementedException();
        }

        public MenuBarPosition GetMenuBarPosition()
        {
            throw new NotImplementedException();
        }

        public TabPosition GetNewTabPosition()
        {
            throw new NotImplementedException();
        }

        public int GetNewTaskId(int todoId)
        {
            throw new NotImplementedException();
        }

        public int GetNewTodoId()
        {
            throw new NotImplementedException();
        }

        public int GetOriginTabIndex()
        {
            throw new NotImplementedException();
        }

        public string GetSystemVersion()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<TaskOrderList> GetTaskOrderDisplayName()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<TaskOrderList> GetTaskOrderList()
        {
            throw new NotImplementedException();
        }

        public bool IsBeginFromTabList()
        {
            throw new NotImplementedException();
        }

        public bool IsBigIcon()
        {
            throw new NotImplementedException();
        }

        public stt.Task OpenConnectionAsync()
        {
            throw new NotImplementedException();
        }

        public stt.Task RenameTaskAsync(int todoId, int taskId, string newName)
        {
            throw new NotImplementedException();
        }

        public stt.Task RenameTodoAsync(int todoId, string newName)
        {
            throw new NotImplementedException();
        }

        public stt.Task ReorderTaskAsync(int todoId, IEnumerable<TodoTask> todoTasks)
        {
            throw new NotImplementedException();
        }

        public stt.Task ReorderTodoAsync(IEnumerable<TodoItem> todoItems)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public stt.Task<IEnumerable<TodoTask>> SelectTaskAllAsync(int todoId)
        {
            throw new NotImplementedException();
        }

        public stt.Task<IEnumerable<TodoItem>> SelectTodoAllAsync()
        {
            throw new NotImplementedException();
        }

        public stt.Task SetDefaultTaskOrderAsync(TaskOrderPattern order)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetDefaultUseTristateAsync(bool value)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetLastPageAsync(ViewingPage viewing)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetLastTabIndexAsync(int focus)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetMenuBarPositionAsync(MenuBarPosition position)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetNewTabPositionAsync(TabPosition position)
        {
            throw new NotImplementedException();
        }

        public stt.Task SetOriginTabIndexAsync(int origin)
        {
            throw new NotImplementedException();
        }

        public stt.Task ToggleTaskStatusAsync(int todoId, int taskId, TaskStatus status)
        {
            throw new NotImplementedException();
        }

        public stt.Task UpdateTaskAsync(int todoId, TodoTask todoTask)
        {
            throw new NotImplementedException();
        }

        public stt.Task UpdateTodoAsync(TodoItem todoItem)
        {
            throw new NotImplementedException();
        }

        public stt.Task UseBigIconAsync(bool usage)
        {
            throw new NotImplementedException();
        }

        #region Mapping
        public static void PrepareMapping()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.RecognizePrefixes("inner_");
                cfg.RecognizeDestinationPrefixes("inner_");
                cfg.CreateMap<string, TaskOrderPattern>().ConvertUsing(s => s.EnumValue<TaskOrderPattern>());
                cfg.CreateMap<string, TaskStatus>().ConvertUsing(s => s.EnumValue<TaskStatus>());
                cfg.CreateMap<int, Color>().ConvertUsing(c => Color.FromArgb(c));
                cfg.CreateMap<TaskOrderPattern, string>().ConvertUsing(p => p.EnumName());
                cfg.CreateMap<TaskStatus, string>().ConvertUsing(s => s.EnumName());
                //cfg.CreateMap<Todo, TodoItem>();
                //cfg.CreateMap<Task, TodoTask>();
                //cfg.CreateMap<TodoItem, Todo>();
                //cfg.CreateMap<TodoTask, Task>();
                //cfg.CreateMap<TaskOrderDisplayName, TaskOrderList>();
                //cfg.CreateMap<IconPatternMaster, IconSetting>();
                //cfg.CreateMap<ColorPatternMaster, ColorSetting>();
            });
        }
        #endregion
    }
}
