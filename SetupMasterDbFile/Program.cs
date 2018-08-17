using System;
using System.Collections.Generic;
using System.IO;
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
                Console.WriteLine("Setup IconMaster");

                Console.WriteLine("Setup ColorMaster");

                Console.WriteLine("Setup SystemVersion");

                realm.Add(new SystemVersion { Version = this.GetType().Assembly.GetName().Version.ToString() });

                Console.WriteLine("Setup SystemSettings");

                Console.WriteLine("Setup LastPage");

                Console.WriteLine("Setup MenuIconMaster");

                Console.WriteLine("Setup TaskDisplayName");

                Console.WriteLine("Setup TodoIdMaster");

                Console.WriteLine("Setup TaskIdMaster");

                transaction.Commit();
            }

            Console.WriteLine("Disconnect");

            realm.Dispose();

            Console.WriteLine("Finished");
        }

        private IEnumerable<IconPatternMaster> GetIconRecords()
        {
            var icons = new List<IconPatternMaster>
            {

            };
            return icons;
        }

        private IEnumerable<ColorPatternMaster> GetColorRecords()
        {
            var colors = new List<ColorPatternMaster>
            {

            };
            return colors;
        }
    }
}
