using System;
using System.Windows.Forms;
using PasswordManager.Database;

namespace PasswordManager.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblUsername;
        private Label lblPassword;
        private Button buttonSignUp;
        private CheckBox chkShowPassword;

        public LoginForm()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            AddSignUpButton();
            this.FormClosing += LoginForm_FormClosing;

            // Add these lines for better appearance
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Password Manager - Login";
        }

        private void InitializeComponent()
        {
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            lblUsername = new Label();
            lblPassword = new Label();
            chkShowPassword = new CheckBox();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(150, 50);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Enter username";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(150, 90);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Enter password";
            txtPassword.Size = new Size(200, 23);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(150, 130);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(100, 30);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Login";
            btnLogin.Click += btnLogin_Click;
            // 
            // lblUsername
            // 
            lblUsername.Location = new Point(50, 53);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(100, 23);
            lblUsername.TabIndex = 4;
            lblUsername.Text = "Username:";
            // 
            // lblPassword
            // 
            lblPassword.Location = new Point(50, 93);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(100, 23);
            lblPassword.TabIndex = 5;
            lblPassword.Text = "Password:";
            // 
            // chkShowPassword
            // 
            chkShowPassword.Location = new Point(360, 90);
            chkShowPassword.Name = "chkShowPassword";
            chkShowPassword.Size = new Size(120, 24);
            chkShowPassword.TabIndex = 2;
            chkShowPassword.Text = "Show Password";
            chkShowPassword.CheckedChanged += chkShowPassword_CheckedChanged;
            // 
            // LoginForm
            // 
            ClientSize = new Size(482, 250);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(chkShowPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
        private void AddSignUpButton()
        {
            buttonSignUp = new Button();
            buttonSignUp.Location = new Point(150, 170);
            buttonSignUp.Size = new Size(100, 30);
            buttonSignUp.Text = "Sign Up";
            buttonSignUp.Click += new EventHandler(ButtonSignUp_Click);
            Controls.Add(buttonSignUp);
        }

        private void ButtonSignUp_Click(object sender, EventArgs e)
        {
            var signUpForm = new SignUpForm();
            if (signUpForm.ShowDialog() == DialogResult.OK)
            {
                txtUsername.Clear();
                txtPassword.Clear();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string role;
                if (DatabaseHelper.ValidateUser(username, password, out role))
                {
                    MainForm mainForm = new MainForm(username, role);
                    this.Hide();
                    mainForm.ShowDialog();
                    Application.Exit(); // Add this to properly exit the application
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "Login Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
