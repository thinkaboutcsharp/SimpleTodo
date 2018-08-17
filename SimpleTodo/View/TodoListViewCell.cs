using System;
using Reactive.Bindings;

using Xamarin.Forms;
using mr = MR.Gestures;

namespace SimpleTodo
{
    public class TodoListViewCell : mr.ViewCell
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
            var clearSelectionTarget = new ClearSelectionOvserver(bc =>
            {
                View.BackgroundColor = bc;
            });
            var visibleSwitchOnOffTarget = new VisibleSwitchOnOffObserver(v =>
            {
                IsSelected.Value = v;
            });

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget((int)RxSourceEnum.ClearListViewSelection, clearSelectionTarget);
            router.AddReactiveTarget((int)RxSourceEnum.VisibleSwitchOnOff, visibleSwitchOnOffTarget);
        }
    }
}

