using System;
using System.Windows.Forms;
using PasswordManager.Database;

namespace PasswordManager.Forms
{
    public partial class SignUpForm : Form
    {
        private TextBox textBoxNewUsername;
        private TextBox textBoxNewPassword;
        private TextBox textBoxConfirmPassword;
        private Button buttonSignUp;
        private Label labelNewUsername;
        private Label labelNewPassword;
        private Label labelConfirmPassword;

        public SignUpForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Initialize controls
            textBoxNewUsername = new TextBox();
            textBoxNewPassword = new TextBox();
            textBoxConfirmPassword = new TextBox();
            buttonSignUp = new Button();
            labelNewUsername = new Label();
            labelNewPassword = new Label();
            labelConfirmPassword = new Label();

            // Label New Username
            labelNewUsername.Location = new Point(50, 53);
            labelNewUsername.Size = new Size(100, 23);
            labelNewUsername.Text = "Username:";

            // TextBox New Username
            textBoxNewUsername.Location = new Point(160, 50);
            textBoxNewUsername.Size = new Size(200, 23);

            // Label New Password
            labelNewPassword.Location = new Point(50, 93);
            labelNewPassword.Size = new Size(100, 23);
            labelNewPassword.Text = "Password:";

            // TextBox New Password
            textBoxNewPassword.Location = new Point(160, 90);
            textBoxNewPassword.Size = new Size(200, 23);
            textBoxNewPassword.UseSystemPasswordChar = true;

            // Label Confirm Password
            labelConfirmPassword.Location = new Point(50, 133);
            labelConfirmPassword.Size = new Size(100, 23);
            labelConfirmPassword.Text = "Confirm Password:";

            // TextBox Confirm Password
            textBoxConfirmPassword.Location = new Point(160, 130);
            textBoxConfirmPassword.Size = new Size(200, 23);
            textBoxConfirmPassword.UseSystemPasswordChar = true;

            // Button Sign Up
            buttonSignUp.Location = new Point(160, 170);
            buttonSignUp.Size = new Size(100, 30);
            buttonSignUp.Text = "Sign Up";
            buttonSignUp.Click += new EventHandler(ButtonSignUp_Click);

            // SignUpForm
            ClientSize = new Size(400, 250);
            Controls.AddRange(new Control[] {
                labelNewUsername,
                textBoxNewUsername,
                labelNewPassword,
                textBoxNewPassword,
                labelConfirmPassword,
                textBoxConfirmPassword,
                buttonSignUp
            });
            Name = "SignUpForm";
            Text = "Sign Up";
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void ButtonSignUp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxNewUsername.Text) ||
                string.IsNullOrEmpty(textBoxNewPassword.Text) ||
                string.IsNullOrEmpty(textBoxConfirmPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (textBoxNewPassword.Text != textBoxConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            try
            {
                if (DatabaseHelper.UserExists(textBoxNewUsername.Text))
                {
                    MessageBox.Show("Username already exists!");
                    return;
                }

                DatabaseHelper.AddUser(textBoxNewUsername.Text, textBoxNewPassword.Text);
                MessageBox.Show("Account created successfully!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating account: {ex.Message}");
            }
        }
    }
}
