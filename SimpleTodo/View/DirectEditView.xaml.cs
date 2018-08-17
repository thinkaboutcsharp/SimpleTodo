using System;
using System.Collections.Generic;
using Reactive.Bindings;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class DirectEditView : ContentView
    {
        public const double BackgroundOpacity = 0.2;

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("名前");
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>(string.Empty);

        public DirectEditMode EditMode { get; set; }
        public bool HasName { get; private set; }

        public event EventHandler Fixed;

        public DirectEditView()
        {
            InitializeComponent();

            BindingContext = this;
            frm_Editor.BorderColor = Color.Accent;

            EditMode = DirectEditMode.New;
        }

        public void SetNewMode()
        {
            EditMode = DirectEditMode.New;
            Title.Value = "新しい名前";
            Name.Value = string.Empty;
        }

        public void SetUpdateMode(string name)
        {
            EditMode = DirectEditMode.Update;
            Title.Value = "名前の変更";
            Name.Value = name;
        }

        private void OnClicked(object sender, EventArgs args)
        {
            HasName = !string.IsNullOrEmpty(Name.Value);
            Fixed?.Invoke(this, new FixedEventArgs(EditMode));
        }
    }

    public class FixedEventArgs : EventArgs
    {
        public DirectEditMode EditMode { get; }
        public FixedEventArgs(DirectEditMode mode) { EditMode = mode; }
    }

    public enum DirectEditMode
    {
        New,
        Update,
    }
}
