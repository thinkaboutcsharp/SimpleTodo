using sys = System.Drawing;
using xam = Xamarin.Forms;
namespace SimpleTodo
{
    public static class SystemColorEx
    {
        public static xam.Color ToXamarinColor(ref sys.Color color) => color;
    }
}
