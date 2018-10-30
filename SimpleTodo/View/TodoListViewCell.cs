using System;
using Reactive.Bindings;

using Xamarin.Forms;

using StyledCells.Gesture;

namespace SimpleTodo
{
    public class TodoListViewCell : StyledGestureViewCell
    {
        public static BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemIdProperty), typeof(int), typeof(TodoListViewCell), default(int), BindingMode.OneWay);
        public int ItemId
        {
            get { return (int)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }

        public ReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>(false);

        public TodoListViewCell()
        {
            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget(RxSourceEnum.ClearListViewSelection, (Color bc) => View.BackgroundColor = bc);
            router.AddReactiveTarget(RxSourceEnum.VisibleSwitchOnOff, (bool v) => IsSelected.Value = v);
        }
    }
}

