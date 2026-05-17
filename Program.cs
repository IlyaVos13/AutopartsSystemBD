using AutopartsSystemBD;
using System;
using System.Windows.Forms;

namespace AutopartsSystemBD
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DataStore.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных PostgreSQL.\n\n" +
                    "Проверьте, что PostgreSQL запущен, база autoparts_db создана, " +
                    "таблицы есть, а пароль в DataStore.cs указан правильно.\n\n" +
                    "Ошибка: " + ex.Message,
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            using var loginForm = new LoginForm();

            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.CurrentUser != null)
            {
                Application.Run(new MainForm(loginForm.CurrentUser));
            }
        }
    }
}