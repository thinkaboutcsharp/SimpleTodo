using System.ComponentModel;

namespace SimpleTodo
{
    class TodoItem : INotifyPropertyChanged
    {
        public TodoItem(string text, bool done) => (_Text, _Done) = (text, done);

        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        private bool _Done;
        public bool Done
        {
            get => _Done;
            set
            {
                _Done = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Done)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
