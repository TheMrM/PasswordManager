using PasswordManager;
using PasswordManager.Forms;

namespace PasswordManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            PasswordManager.ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
