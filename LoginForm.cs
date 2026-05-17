using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutopartsSystemBD
{
    public class LoginForm : Form
    {
        private readonly TextBox loginBox = new TextBox();
        private readonly TextBox passwordBox = new TextBox();
        private readonly Button loginButton = new Button();

        public User? CurrentUser { get; private set; }

        public LoginForm()
        {
            Text = "Вход в систему";
            Width = 360;
            Height = 230;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var titleLabel = new Label();
            titleLabel.Text = "Авторизация";
            titleLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            titleLabel.SetBounds(110, 20, 200, 30);
            Controls.Add(titleLabel);

            var loginLabel = new Label();
            loginLabel.Text = "Логин:";
            loginLabel.SetBounds(40, 70, 100, 25);
            Controls.Add(loginLabel);

            loginBox.SetBounds(140, 70, 150, 25);
            Controls.Add(loginBox);

            var passwordLabel = new Label();
            passwordLabel.Text = "Пароль:";
            passwordLabel.SetBounds(40, 105, 100, 25);
            Controls.Add(passwordLabel);

            passwordBox.SetBounds(140, 105, 150, 25);
            passwordBox.PasswordChar = '*';
            Controls.Add(passwordBox);

            loginButton.Text = "Войти";
            loginButton.SetBounds(140, 145, 150, 35);
            loginButton.Click += LoginButton_Click;
            Controls.Add(loginButton);

            AcceptButton = loginButton;
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            string login = loginBox.Text.Trim();
            string password = passwordBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            try
            {
                var user = DataStore.Login(login, password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль");
                    return;
                }

                CurrentUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка входа: " + ex.Message);
            }
        }
    }
}