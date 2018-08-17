using System;

using Xamarin.Forms;
using RxRouting;

namespace SimpleTodo
{
    public class EmptyPage : ContentPage, ITabPage
    {
        private TodoItem setting;
        public TodoItem Setting { get => setting; set { setting = value; Title = value.Name.Value; } }

        private double lastHeight;
        private double lastWidth;

        private PageRotetionObservable source = new PageRotetionObservable();

        public EmptyPage()
        {
            var router = Application.Current.ReactionRouter();
            router.AddReactiveSource((int)RxSourceEnum.PageRotation, source);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (IsAlmostEquals(lastWidth, width) && IsAlmostEquals(lastHeight, height)) return;

            lastWidth = width;
            lastHeight = height;

            if (Height > Width) source.Send(PageDirectionEnum.Vertical);
            else source.Send(PageDirectionEnum.Horizontal);
        }

        private bool IsAlmostEquals(double a, double b) => Math.Abs(a - b) < 10e-3 ? true : false;

        class PageRotetionObservable : ObservableBase<PageDirectionEnum>
        {
        }
    }
}