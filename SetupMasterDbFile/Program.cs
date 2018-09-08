using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleTodo;
using Realms;

namespace SetupMasterDbFile
{
    class Program
    {
        const string DatabasePath = "../SimpleTodo/Data/master.realm";

        static void Main(string[] args)
        {
            var program = new Program();
            program.Setup(ulong.Parse(args[0]));
        }

        private void Setup(ulong version)
        {
            Console.WriteLine("Start & Connect Schema={0}", version);

            if (File.Exists(DatabasePath))
                File.Open(DatabasePath, FileMode.Truncate).Close();

            var config = new RealmConfiguration(Path.GetFullPath(DatabasePath));
            config.SchemaVersion = version;
            config.ObjectClasses = new Type[]
            {
                typeof(SystemVersion),
                typeof(SystemSettings),
                typeof(LastPage),
                typeof(TodoIdMaster),
                typeof(TaskIdMaster),
                typeof(MenuIconMaster),
                typeof(IconPatternMaster),
                typeof(ColorPatternMaster),
                typeof(TaskOrderDisplayName),
                typeof(Todo),
                typeof(Task)
            };
            var realm = Realm.GetInstance(config);

            using (var transaction = realm.BeginWrite())
            {
                #region Upgrade
                Console.WriteLine("Setup IconMaster");

                var icons = GetIconRecords();
                foreach (var icon in icons) realm.Add(icon);

                Console.WriteLine("Setup ColorMaster");

                var colors = GetColorRecords();
                foreach (var color in colors) realm.Add(color);

                Console.WriteLine("Setup SystemVersion");

                realm.Add(new SystemVersion { Version = typeof(SimpleTodo.App).Assembly.GetName().Version.ToString() });
                #endregion

                #region Initialize
                Console.WriteLine("Setup SystemSettings");

                var system = new SystemSettings
                {
                    UseBigIcon = false,
                    BeginFromTabList = false,
                    HorizontalMenuBarPosition = "Left",
                    NewTabPosition = "Top",
                    DefaultUseTristate = false,
                    DefaultTaskOrder = "Registered",
                    DefaultIconPattern = 0,
                    DefaultColorPattern = 0
                };
                realm.Add(system);

                Console.WriteLine("Setup LastPage");

                var last = new LastPage
                {
                    LastViewing = "Todo",
                    LastFocus = 0,
                    Origin = 0
                };
                realm.Add(last);

                Console.WriteLine("Setup MenuIconMaster");

                var menu1 = new MenuIconMaster()
                {
                    TabListIcon = "tablist.png",
                    NewTaskIcon = "newtask.png",
                    Up = "up.png",
                    Down = "down.png",
                    TabSetting = "setting.png",
                    NewTabIcon = "newtab.png",
                    SwitchOnOff = "switch.png"
                };
                realm.Add(menu1);

                Console.WriteLine("Setup TaskOrderDisplayName");

                var display1 = new TaskOrderDisplayName
                {
                    TaskOrder = "Registered",
                    DisplayName = "登録順"
                };
                var display2 = new TaskOrderDisplayName
                {
                    TaskOrder = "Name",
                    DisplayName = "名前順"
                };
                realm.Add(display1);
                realm.Add(display2);

                Console.WriteLine("Setup TodoIdMaster");

                realm.Add(new TodoIdMaster { NextTodoId = 0 });

                Console.WriteLine("Setup TaskIdMaster");
                //Noop
                #endregion

                transaction.Commit();
            }

            Console.WriteLine("Disconnect");

            realm.Dispose();

            Console.WriteLine("Finished");
        }

        private IEnumerable<IconPatternMaster> GetIconRecords()
        {
            var mapper = new PropertyMapper();
            mapper.AddConfig<int>("IconId", s => int.Parse(s));

            var icons = ReadMasterFile<IconPatternMaster>("icon.txt", mapper);
            return icons;
        }

        private IEnumerable<ColorPatternMaster> GetColorRecords()
        {
            var mapper = new PropertyMapper();
            mapper.AddConfig<int>(string.Empty, s => int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier));

            var colors = ReadMasterFile<ColorPatternMaster>("color.txt", mapper);
            return colors;
        }

        private IEnumerable<T> ReadMasterFile<T>(string fileName, PropertyMapper mapper)
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties();
            var header = new List<string>();
            using (var master = new StreamReader(new FileStream("Master/" + fileName, FileMode.Open)))
            {
                var line = master.ReadLine();

                //ヘッダー読み込み
                var headers = line.Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    var propertyName = headers[i].Trim();
                    header.Add(propertyName);
                }

                //データ読み込み
                while ((line = master.ReadLine()) != null)
                {
                    var newObject = Activator.CreateInstance<T>();
                    var datas = line.Split(',');
                    for (int i = 0; i < header.Count; i++)
                    {
                        mapper.Map(header[i], datas[i].Trim(), out var data);
                        var propertyInfo = properties.Where(p => p.Name == header[i]).FirstOrDefault();
                        propertyInfo?.SetValue(newObject, data);
                    }
                    result.Add(newObject);
                }
            }
            return result;
        }

        class PropertyMapper
        {
            private Dictionary<string, Delegate> mappConfig = new Dictionary<string, Delegate>();
            public void AddConfig<T>(string propertyName, Func<string, T> mapper)
            {
                mappConfig.Add(propertyName, mapper);
            }
            public void Map(string propertyName, string input, out object output)
            {
                if (mappConfig.ContainsKey(propertyName))
                {
                    output = mappConfig[propertyName].DynamicInvoke(input);
                }
                else if (mappConfig.ContainsKey(string.Empty))
                {
                    output = mappConfig[string.Empty].DynamicInvoke(input);
                }
                else
                {
                    output = input;
                }
            }
        }
    }
}
