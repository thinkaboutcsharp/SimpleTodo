using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class MenuBarView : ContentView
    {
        #region BindableProperties
        public enum MenuBarItems
        {
            Item1,
            Item2,
            Item3,
            Item4,
            Item5,
            End
        }

        public static BindableProperty MenuBarItem1Property
        = BindableProperty.Create(nameof(MenuBarItem1Property), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.Item1));
        public MenuBarItem MenuBarItem1
        {
            get { return (MenuBarItem)GetValue(MenuBarItem1Property); }
            set { SetValue(MenuBarItem1Property, value); }
        }

        public static BindableProperty MenuBarItem2Property
        = BindableProperty.Create(nameof(MenuBarItem2Property), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.Item2));
        public MenuBarItem MenuBarItem2
        {
            get { return (MenuBarItem)GetValue(MenuBarItem2Property); }
            set { SetValue(MenuBarItem2Property, value); }
        }

        public static BindableProperty MenuBarItem3Property
        = BindableProperty.Create(nameof(MenuBarItem3Property), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.Item3));
        public MenuBarItem MenuBarItem3
        {
            get { return (MenuBarItem)GetValue(MenuBarItem3Property); }
            set { SetValue(MenuBarItem3Property, value); }
        }

        public static BindableProperty MenuBarItem4Property
        = BindableProperty.Create(nameof(MenuBarItem4Property), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.Item4));
        public MenuBarItem MenuBarItem4
        {
            get { return (MenuBarItem)GetValue(MenuBarItem4Property); }
            set { SetValue(MenuBarItem4Property, value); }
        }

        public static BindableProperty MenuBarItem5Property
        = BindableProperty.Create(nameof(MenuBarItem5Property), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.Item5));
        public MenuBarItem MenuBarItem5
        {
            get { return (MenuBarItem)GetValue(MenuBarItem5Property); }
            set { SetValue(MenuBarItem5Property, value); }
        }

        public static BindableProperty MenuBarItemEndProperty
        = BindableProperty.Create(nameof(MenuBarItemEndProperty), typeof(MenuBarItem), typeof(MenuBarView), null, propertyChanged: (b, o, n) => OnMenuBarItemChanged(b, o, n, MenuBarItems.End));
        public MenuBarItem MenuBarItemEnd
        {
            get { return (MenuBarItem)GetValue(MenuBarItemEndProperty); }
            set { SetValue(MenuBarItemEndProperty, value); }
        }
        #endregion

        private const int BigIconSize = 30;
        private const int StandardIconSize = 25;

        private PageRotationOvserver pageRotationTarget;
        private MenuBarIconSizeChangedOvserver iconSizeChangedOvserver;

		private MenuBarViewModel model = new MenuBarViewModel(Application.Current.RealmAccess());

        public MenuBarView()
        {
            InitializeComponent();

            //全く美しくないが、確実に配置するためには今のところこれが一番いい
            var layout = (StackLayout)Content;
            for (int i = 0; i < 5; i++)
            {
                var box = MakePlaceholder();
                layout.Children.Add(box);
            }

            var boxEnd = MakePlaceholder();
            boxEnd.HorizontalOptions = LayoutOptions.EndAndExpand;
            layout.Children.Add(boxEnd);

            pageRotationTarget = new PageRotationOvserver(d => OnRotation(d));
            iconSizeChangedOvserver = new MenuBarIconSizeChangedOvserver(b => OnMenuBarIconSizeChanged(b));
            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget<PageDirectionEnum>((int)RxSourceEnum.PageRotation, pageRotationTarget);
            router.AddReactiveTarget<bool>((int)RxSourceEnum.MenuBarIconSizeChange, iconSizeChangedOvserver);
        }

        private static void OnMenuBarItemChanged(BindableObject bindable, object oldValue, object newValue, MenuBarItems property)
        {
            ((MenuBarView)bindable).DrawMenuBar(property);
        }

        private void DrawMenuBar(MenuBarItems property)
        {
            var layout = (StackLayout)Content;

            switch (property)
            {
                case MenuBarItems.Item1:
                    layout.Children[0] = MakeImage(MenuBarItem1);
                    break;
                case MenuBarItems.Item2:
                    layout.Children[1] = MakeImage(MenuBarItem2);
                    break;
                case MenuBarItems.Item3:
                    layout.Children[2] = MakeImage(MenuBarItem3);
                    break;
                case MenuBarItems.Item4:
                    layout.Children[3] = MakeImage(MenuBarItem4);
                    break;
                case MenuBarItems.Item5:
                    layout.Children[4] = MakeImage(MenuBarItem5);
                    break;
                case MenuBarItems.End:
                    {
                        var endImage = MakeImage(MenuBarItemEnd);
                        if (endImage is View view) view.HorizontalOptions = LayoutOptions.EndAndExpand;
                        layout.Children[5] = endImage;
                        break;
                    }
            }
        }

        private View MakeImage(MenuBarItem menu)
        {
            if (menu == null) return MakePlaceholder();

            var image = new Image
            {
                Source = ImageSource.FromResource(this.GetType().Namespace + ".Image." + menu.ImagePath, this.GetType().Assembly),
                HeightRequest = model.NeedBigIcon ? BigIconSize : StandardIconSize,
                WidthRequest = model.NeedBigIcon ? BigIconSize : StandardIconSize,
                BackgroundColor = Color.BlueViolet
            };
            var gesture = new TapGestureRecognizer
            {
                Command = menu.TappedCommand
            };
            image.GestureRecognizers.Add(gesture);
            return image;
        }

        private void OnMenuBarIconSizeChanged(bool needBigIcon)
        {
            model.NeedBigIcon = needBigIcon;
            DrawMenuBarIcons();
        }

        private void DrawMenuBarIcons()
        {
            DrawMenuBar(MenuBarItems.Item1);
            DrawMenuBar(MenuBarItems.Item2);
            DrawMenuBar(MenuBarItems.Item3);
            DrawMenuBar(MenuBarItems.Item4);
            DrawMenuBar(MenuBarItems.Item5);
            DrawMenuBar(MenuBarItems.End);
        }

        private void OnRotation(PageDirectionEnum pageDirection)
        {
            switch (pageDirection)
            {
                case PageDirectionEnum.Horizontal:
                    RotateToHorizontal();
                    break;
                case PageDirectionEnum.Vertical:
                    RotateToVertical();
                    break;
            }
        }

        private void RotateToVertical()
        {
            var layout = (StackLayout)Content;
            layout.Orientation = StackOrientation.Horizontal;
            layout.HeightRequest = model.NeedBigIcon ? 40 : 35;
        }

        private void RotateToHorizontal()
        {
            var layout = (StackLayout)Content;
            layout.Orientation = StackOrientation.Vertical;
            layout.WidthRequest = model.NeedBigIcon ? 40 : 35;
        }

        private View MakePlaceholder()
        {
            var placeholder = new BoxView
            {
                BackgroundColor = Color.Transparent,
                HeightRequest = WidthRequest = model.NeedBigIcon ? BigIconSize : StandardIconSize,
                WidthRequest = WidthRequest = model.NeedBigIcon ? BigIconSize : StandardIconSize
            };
            return placeholder;
        }
    }

    public class MenuBarItem
    {
        public string ImagePath { get; set; }
        public ICommand TappedCommand { get; set; }
    }
}
