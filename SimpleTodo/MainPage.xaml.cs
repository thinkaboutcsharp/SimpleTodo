using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            InitializeComponent();

            var dummy = new ContentPage
            {
                Title = "メニュー",
                Padding = new Thickness(0, 80, 0, 0),
                Content = new StackLayout
                {
                    Children =
                    {
                        new Label { Text = "スライドメニュー1", VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 10)},
                        new Label { Text = "スライドメニュー2", VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 10)},
                        new Label { Text = "スライドメニュー3", VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 10)},
                        new Label { Text = "スライドメニュー4", VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 10)},
                        new Label { Text = "スライドメニュー5", VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 10)}
                    }
                }
            };
            Master = dummy;

            var tabView = new TabViewPage();
            tabView.Children.Insert(0, new TemplatePage("さいしょ"));
            tabView.CurrentPage = tabView.Children[0];
            Detail = new NavigationPage(tabView);
        }
    }
}
