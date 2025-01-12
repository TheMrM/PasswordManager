using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using PasswordManager.Database;

namespace PasswordManager.Forms
{
    public partial class MainForm : Form
    {
        private string currentUsername;
        private string userRole;
        private Panel sideMenu;
        private Panel contentPanel;
        private Label welcomeLabel;
        private Button btnUserInfo;
        private Button btnPasswords;
        private Button btnSettings;
        private Button btnManageUsers;
        private Button btnLogout;

        // Method for indicating pasword strenght
        private (string strength, Color color) CheckPasswordStrength(string password)
        {
            int score = 0;
            if (string.IsNullOrEmpty(password)) return ("Very Weak", Color.Red);

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) score++;

            return score switch
            {
                0 => ("Very Weak", Color.Red),
                1 or 2 => ("Weak", Color.OrangeRed),
                3 => ("Medium", Color.Orange),
                4 => ("Strong", Color.YellowGreen),
                _ => ("Very Strong", Color.Green)
            };
        }

        // Secure password generator method
        private string GenerateSecurePassword(int length = 16)
        {
            const string upperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijkmnopqrstuvwxyz";
            const string numeric = "23456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one character from each category
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(numeric[random.Next(numeric.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Fill the rest randomly
            var allChars = upperCase + lowerCase + numeric + special;
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().ToCharArray()
                .OrderBy(x => random.Next()).ToArray());
        }

        // Check is URL is valid so it's safe
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttps
                && !string.IsNullOrWhiteSpace(uriResult.Host)
                && uriResult.Host.Contains(".");
        }

        public MainForm(string username, string role)
        {
            currentUsername = username;
            userRole = role;
            InitializeComponent();
            SetupMainForm();
        }

        private void InitializeComponent()
        {
            this.sideMenu = new Panel();
            this.contentPanel = new Panel();
            this.welcomeLabel = new Label();
            this.btnUserInfo = new Button();
            this.btnPasswords = new Button();
            this.btnSettings = new Button();
            this.btnLogout = new Button();

            // Form settings
            this.Text = "Password Manager - Main Menu";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Side Menu Panel
            sideMenu.BackColor = Color.FromArgb(51, 51, 76);
            sideMenu.Dock = DockStyle.Left;
            sideMenu.Width = 200;

            // Welcome Label
            welcomeLabel.Text = $"Welcome, {currentUsername}!";
            welcomeLabel.ForeColor = Color.White;
            welcomeLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new Point(10, 20);
            sideMenu.Controls.Add(welcomeLabel);

            // Helper method for button setup
            void SetupButton(Button btn, string text, int yPosition)
            {
                btn.Text = text;
                btn.Size = new Size(180, 40);
                btn.Location = new Point(10, yPosition);
                btn.FlatStyle = FlatStyle.Flat;
                btn.ForeColor = Color.White;
                btn.BackColor = Color.FromArgb(51, 51, 76);
                btn.FlatAppearance.BorderSize = 0;

                // Add hover effect
                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(71, 71, 96);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(51, 51, 76);

                sideMenu.Controls.Add(btn);
            }

            // Setup all buttons
            int startY = 80;
            SetupButton(btnUserInfo, "User Information", startY);
            btnUserInfo.Click += BtnUserInfo_Click;

            SetupButton(btnPasswords, "Manage Passwords", startY + 50);
            btnPasswords.Click += BtnPasswords_Click;

            SetupButton(btnSettings, "Settings", startY + 100);
            btnSettings.Click += BtnSettings_Click;

            // Add Manage Users button for admin
            if (userRole.ToLower() == "admin")
            {
                btnManageUsers = new Button();
                SetupButton(btnManageUsers, "Manage Users", startY + 150);
                btnManageUsers.Click += BtnManageUsers_Click;
            }

            // Logout button at the bottom
            SetupButton(btnLogout, "Logout", this.Height - 100);
            btnLogout.Click += BtnLogout_Click;

            // Content Panel
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.White;

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sideMenu);

        }

        private void SetupMainForm()
        {
            ShowUserInfo(); // Show user info by default
        }

        private void ShowUserInfo()
        {
            contentPanel.Controls.Clear();

            Label titleLabel = new Label
            {
                Text = "User Information",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            Label usernameLabel = new Label
            {
                Text = $"Username: {currentUsername}",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(20, 60)
            };

            // Add more user information as needed
            contentPanel.Controls.Add(titleLabel);
            contentPanel.Controls.Add(usernameLabel);
        }

        private void BtnManageUsers_Click(object sender, EventArgs e)
        {
            contentPanel.Controls.Clear();

            // Title
            Label titleLabel = new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            contentPanel.Controls.Add(titleLabel);

            // Add New User Button
            Button btnAddUser = new Button
            {
                Text = "Add New User",
                Size = new Size(120, 30),
                Location = new Point(20, 60),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddUser.Click += BtnAddUser_Click;
            contentPanel.Controls.Add(btnAddUser);

            // DataGridView for users
            DataGridView dgvUsers = new DataGridView
            {
                Name = "dgvUsers",
                Location = new Point(20, 100),
                Size = new Size(contentPanel.Width - 40, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            // Set up columns first
            dgvUsers.Columns.Add("Username", "Username");
            dgvUsers.Columns.Add("Role", "Role");
            dgvUsers.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "Edit",
                Text = "Edit",
                UseColumnTextForButtonValue = true
            });
            dgvUsers.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "Delete",
                Text = "Delete",
                UseColumnTextForButtonValue = true
            });

            contentPanel.Controls.Add(dgvUsers);

            // Handle cell click events
            dgvUsers.CellClick += (s, evt) =>
            {
                if (evt.RowIndex < 0) return;

                var username = dgvUsers.Rows[evt.RowIndex].Cells["Username"].Value?.ToString();
                if (string.IsNullOrEmpty(username)) return;

                // Edit button clicked
                if (evt.ColumnIndex == dgvUsers.Columns["Edit"].Index)
                {
                    EditUser(username);
                }
                // Delete button clicked
                else if (evt.ColumnIndex == dgvUsers.Columns["Delete"].Index)
                {
                    if (username == "admin")
                    {
                        MessageBox.Show("Cannot delete admin user!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (MessageBox.Show($"Are you sure you want to delete user '{username}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (DatabaseHelper.DeleteUser(username))
                        {
                            MessageBox.Show("User deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshUserList(dgvUsers);
                        }
                    }
                }
            };

            RefreshUserList(dgvUsers);
        }

        private void RefreshUserList(DataGridView dgv)
        {
            dgv.Rows.Clear();
            var users = DatabaseHelper.GetAllUsers();
            foreach (var user in users)
            {
                dgv.Rows.Add(user.Username, user.Role);
            }
        }


        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            var addUserForm = new Form
            {
                Text = "Add New User",
                Size = new Size(500, 300),  // Increased width
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Style for labels
            var labelStyle = new
            {
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(51, 51, 76)
            };

            // Style for input controls
            var inputStyle = new
            {
                Width = 250,
                Height = 30,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 51, 76)
            };

            // Labels
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(40, 40),
                Width = labelStyle.Width,
                Height = labelStyle.Height,
                Font = labelStyle.Font,
                ForeColor = labelStyle.ForeColor
            };

            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(40, 85),
                Width = labelStyle.Width,
                Height = labelStyle.Height,
                Font = labelStyle.Font,
                ForeColor = labelStyle.ForeColor
            };

            var lblRole = new Label
            {
                Text = "Role:",
                Location = new Point(40, 130),
                Width = labelStyle.Width,
                Height = labelStyle.Height,
                Font = labelStyle.Font,
                ForeColor = labelStyle.ForeColor
            };

            // Input controls
            var txtUsername = new TextBox
            {
                Location = new Point(150, 37),
                Width = inputStyle.Width,
                Height = inputStyle.Height,
                Font = inputStyle.Font,
                BackColor = inputStyle.BackColor,
                ForeColor = inputStyle.ForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            var txtPassword = new TextBox
            {
                Location = new Point(150, 82),
                Width = inputStyle.Width,
                Height = inputStyle.Height,
                Font = inputStyle.Font,
                BackColor = inputStyle.BackColor,
                ForeColor = inputStyle.ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            var cmbRole = new ComboBox
            {
                Location = new Point(150, 127),
                Width = inputStyle.Width,
                Height = inputStyle.Height,
                Font = inputStyle.Font,
                BackColor = inputStyle.BackColor,
                ForeColor = inputStyle.ForeColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new string[] { "basic", "admin" });
            cmbRole.SelectedIndex = 0;

            // Buttons with styling
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(150, 180),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(280, 180),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            // Add controls to form
            addUserForm.Controls.AddRange(new Control[]
            {
        lblUsername, lblPassword, lblRole,
        txtUsername, txtPassword, cmbRole,
        btnSave, btnCancel
            });

            if (addUserForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DatabaseHelper.AddUser(txtUsername.Text, txtPassword.Text, cmbRole.Text);
                    MessageBox.Show("User added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshUserList((DataGridView)contentPanel.Controls.Find("dgvUsers", true)[0]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void EditUser(string username)
        {
            var editUserForm = new Form
            {
                Text = "Edit User",
                Size = new Size(550, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Style for labels
            var labelStyle = new
            {
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(51, 51, 76)
            };

            // Style for input controls
            var inputStyle = new
            {
                Width = 250,
                Height = 30,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 51, 76)
            };

            // Current User Label
            var lblCurrentUser = new Label
            {
                Text = $"Editing user: {username}",
                Location = new Point(40, 20),
                Width = 400,
                Height = labelStyle.Height,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 76)
            };

            // Labels
            var lblPassword = new Label
            {
                Text = "New Password:",
                Location = new Point(40, 60),
                Width = labelStyle.Width,
                Height = labelStyle.Height,
                Font = labelStyle.Font,
                ForeColor = labelStyle.ForeColor
            };

            var lblRole = new Label
            {
                Text = "Role:",
                Location = new Point(40, 105),
                Width = labelStyle.Width,
                Height = labelStyle.Height,
                Font = labelStyle.Font,
                ForeColor = labelStyle.ForeColor
            };

            // Input controls
            var txtPassword = new TextBox
            {
                Location = new Point(150, 57),
                Width = inputStyle.Width,
                Height = inputStyle.Height,
                Font = inputStyle.Font,
                BackColor = inputStyle.BackColor,
                ForeColor = inputStyle.ForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            var cmbRole = new ComboBox
            {
                Location = new Point(150, 102),
                Width = inputStyle.Width,
                Height = inputStyle.Height,
                Font = inputStyle.Font,
                BackColor = inputStyle.BackColor,
                ForeColor = inputStyle.ForeColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new string[] { "basic", "admin" });
            cmbRole.SelectedIndex = 0;

            // Show Password CheckBox
            var chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(410, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(51, 51, 76)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            // Buttons with styling
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(150, 150),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(280, 150),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            // Add controls to form
            editUserForm.Controls.AddRange(new Control[]
            {
        lblCurrentUser,
        lblPassword, lblRole,
        txtPassword, cmbRole,
        chkShowPassword,
        btnSave, btnCancel
            });

            if (editUserForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtPassword.Text))
                    {
                        MessageBox.Show("Please enter a new password.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DatabaseHelper.UpdateUser(username, txtPassword.Text, cmbRole.Text))
                    {
                        MessageBox.Show("User updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshUserList((DataGridView)contentPanel.Controls.Find("dgvUsers", true)[0]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void BtnUserInfo_Click(object sender, EventArgs e)
        {
            ShowUserInfo();
        }

        private void BtnPasswords_Click(object sender, EventArgs e)
        {
            contentPanel.Controls.Clear();

            // Title Label
            Label titleLabel = new Label
            {
                Text = "Password Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            contentPanel.Controls.Add(titleLabel);

            // Add New Password Button
            Button btnAddPassword = new Button
            {
                Text = "Add New Password",
                Size = new Size(150, 35),
                Location = new Point(20, 60),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddPassword.FlatAppearance.BorderSize = 0;
            btnAddPassword.Click += BtnAddPassword_Click;
            contentPanel.Controls.Add(btnAddPassword);

            // Create a DataGridView to display passwords
            DataGridView dgvPasswords = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(contentPanel.Width - 40, contentPanel.Height - 130),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            // Style the DataGridView
            dgvPasswords.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgvPasswords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvPasswords.EnableHeadersVisualStyles = false;
            dgvPasswords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 76);
            dgvPasswords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPasswords.GridColor = Color.FromArgb(230, 230, 230);

            // Define columns for DataGridView
            dgvPasswords.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    Name = "Id",
                    DataPropertyName = "Id",
                    Visible = false
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Website",
                    HeaderText = "Website",
                    DataPropertyName = "Website",
                    Width = 200
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Username",
                    HeaderText = "Username",
                    DataPropertyName = "Username",
                    Width = 200
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Password",
                    HeaderText = "Password",
                    DataPropertyName = "Password",
                    Width = 200
                },
                new DataGridViewButtonColumn
                {
                    Name = "Show",
                    HeaderText = "",
                    Text = "Show",
                    UseColumnTextForButtonValue = true,
                    Width = 70
                },
                new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    Width = 70
                },
                 new DataGridViewButtonColumn
                {
                    Name = "Copy",
                    HeaderText = "",
                    Text = "Copy",
                    UseColumnTextForButtonValue = true,
                    Width = 70
                }
            });

            // Keep track of which passwords are visible
            Dictionary<int, bool> passwordVisibility = new Dictionary<int, bool>();

            // Then update the cell click handler to use the correct column name
            dgvPasswords.CellClick += (s, evt) =>
            {
                if (evt.RowIndex < 0) return;

                var passwordId = Convert.ToInt32(dgvPasswords.Rows[evt.RowIndex].Cells["Id"].Value);

                // Show/Hide password button clicked
                if (evt.ColumnIndex == dgvPasswords.Columns["Show"].Index)
                {
                    if (!passwordVisibility.ContainsKey(evt.RowIndex))
                        passwordVisibility[evt.RowIndex] = false;

                    passwordVisibility[evt.RowIndex] = !passwordVisibility[evt.RowIndex];

                    var passwords = DatabaseHelper.GetStoredPasswords(DatabaseHelper.GetUserId(currentUsername));
                    var password = passwords.First(p => p.Id == passwordId).Password;

                    dgvPasswords.Rows[evt.RowIndex].Cells["Password"].Value =
                        passwordVisibility[evt.RowIndex] ? password : "********";

                    // Update button text (corrected version)
                    dgvPasswords.Rows[evt.RowIndex].Cells["Show"].Value =
                        passwordVisibility[evt.RowIndex] ? "Hide" : "Show";
                }
                // Copy button clicked
                else if (evt.ColumnIndex == dgvPasswords.Columns["Copy"].Index)
                {
                    var passwords = DatabaseHelper.GetStoredPasswords(DatabaseHelper.GetUserId(currentUsername));
                    var password = passwords.First(p => p.Id == passwordId).Password;

                    Clipboard.SetText(password);
                    MessageBox.Show("Password copied to clipboard!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear clipboard after 30 seconds
                    Task.Delay(30000).ContinueWith(t =>
                    {
                        if (Clipboard.GetText() == password)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                Clipboard.Clear();
                            });
                        }
                    });
                }
                // Delete button clicked
                else if (evt.ColumnIndex == dgvPasswords.Columns["Delete"].Index)
                {
                    if (MessageBox.Show("Are you sure you want to delete this password?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        DatabaseHelper.DeleteStoredPassword(passwordId);
                        RefreshPasswordList(dgvPasswords);
                    }
                }
            };


            // Load data
            RefreshPasswordList(dgvPasswords);

            // Add DataGridView to the panel
            contentPanel.Controls.Add(dgvPasswords);
        }

        private void RefreshPasswordList(DataGridView dgv)
        {
            try
            {
                var userId = DatabaseHelper.GetUserId(currentUsername);
                var passwords = DatabaseHelper.GetStoredPasswords(userId);

                dgv.Rows.Clear();
                foreach (var pwd in passwords)
                {
                    dgv.Rows.Add(new object[]
                    {
                pwd.Id,
                pwd.Website,
                pwd.Username,
                "********",
                "Show",
                "Delete"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing password list: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BtnAddPassword_Click(object sender, EventArgs e)
        {
            var addPasswordForm = new Form
            {
                Text = "Add New Password",
                Size = new Size(570, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Style for controls
            var inputStyle = new
            {
                LabelWidth = 80,
                TextBoxWidth = 250,
                Height = 30,
                Padding = 20,
                Font = new Font("Segoe UI", 9.5f)
            };

            // Website
            var lblWebsite = new Label
            {
                Text = "Website:",
                Location = new Point(20, 25),
                Width = inputStyle.LabelWidth,
                Font = inputStyle.Font,
                TextAlign = ContentAlignment.MiddleRight
            };

            var txtWebsite = new TextBox
            {
                Location = new Point(lblWebsite.Right + 10, 25),
                Width = inputStyle.TextBoxWidth,
                Font = inputStyle.Font,
                Height = inputStyle.Height,
                PlaceholderText = "https://"
            };

            // Add validation label (initially invisible)
            var lblValidation = new Label
            {
                Location = new Point(txtWebsite.Left, txtWebsite.Bottom + 2),
                Font = new Font(inputStyle.Font.FontFamily, 8),
                ForeColor = Color.Red,
                AutoSize = true,
                Visible = false
            };

            // Username
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(20, txtWebsite.Bottom + 20),
                Width = inputStyle.LabelWidth,
                Font = inputStyle.Font,
                TextAlign = ContentAlignment.MiddleRight
            };

            var txtUsername = new TextBox
            {
                Location = new Point(lblUsername.Right + 10, lblUsername.Top),
                Width = inputStyle.TextBoxWidth,
                Font = inputStyle.Font,
                Height = inputStyle.Height
            };

            // Password
            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, txtUsername.Bottom + 20),
                Width = inputStyle.LabelWidth,
                Font = inputStyle.Font,
                TextAlign = ContentAlignment.MiddleRight
            };

            var txtPassword = new TextBox
            {
                Location = new Point(lblPassword.Right + 10, lblPassword.Top),
                Width = inputStyle.TextBoxWidth,
                UseSystemPasswordChar = true,
                Font = inputStyle.Font,
                Height = inputStyle.Height
            };

            // Password strength label
            var lblStrength = new Label
            {
                Location = new Point(txtPassword.Right + 10, txtPassword.Top + 5),
                AutoSize = true,
                Font = new Font(inputStyle.Font.FontFamily, 8)
            };

            // Show Password checkbox
            var chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(txtPassword.Left, txtPassword.Bottom + 20),
                AutoSize = true,
                Font = inputStyle.Font
            };

            // Generate Password button
            var btnGeneratePassword = new Button
            {
                Text = "Generate Password",
                Location = new Point(chkShowPassword.Right + 20, chkShowPassword.Top - 4),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGeneratePassword.FlatAppearance.BorderSize = 0;

            // Add the CheckBox event handler
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '•';  // Add this line
            };

            // Button Save
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(txtPassword.Left, chkShowPassword.Bottom + 20),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.FlatAppearance.BorderSize = 0;

            // Button Cancel
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(btnSave.Right + 10, btnSave.Top),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(51, 51, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            // Website validation
            txtWebsite.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtWebsite.Text))
                {
                    if (!IsValidUrl(txtWebsite.Text))
                    {
                        lblValidation.Text = "Please enter a valid HTTPS URL";
                        lblValidation.Visible = true;
                        btnSave.Enabled = false;
                    }
                    else
                    {
                        lblValidation.Visible = false;
                        btnSave.Enabled = true;
                    }
                }
            };

            txtPassword.TextChanged += (s, e) =>
            {
                var (strength, color) = CheckPasswordStrength(txtPassword.Text);
                lblStrength.Text = $"Password Strength: {strength}";
                lblStrength.ForeColor = color;
            };

            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '•';
            };

            btnGeneratePassword.Click += (s, e) =>
            {
                txtPassword.Text = GenerateSecurePassword();
            };

            // Add controls to form
            addPasswordForm.Controls.AddRange(new Control[]
            {
        lblWebsite, txtWebsite, lblValidation,
        lblUsername, txtUsername,
        lblPassword, txtPassword, lblStrength,
        chkShowPassword, btnGeneratePassword,
        btnSave, btnCancel
            });

            // Modify Save button click to include validation
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtWebsite.Text) ||
                    string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please fill in all fields.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    addPasswordForm.DialogResult = DialogResult.None;
                    return;
                }

                if (!txtWebsite.Text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Website must start with 'https://'", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    addPasswordForm.DialogResult = DialogResult.None;
                    return;
                }

                // If we get here, validation passed
                addPasswordForm.DialogResult = DialogResult.OK;
            };
                        
            // Add all controls to the form
            addPasswordForm.Controls.AddRange(new Control[]
            {
                lblWebsite, txtWebsite, lblValidation,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                chkShowPassword,
                btnSave, btnCancel
            });

            if (addPasswordForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Add the new password
                    int userId = DatabaseHelper.GetUserId(currentUsername);
                    DatabaseHelper.AddStoredPassword(userId, txtWebsite.Text, txtUsername.Text, txtPassword.Text);

                    // Get the current DataGridView from the content panel
                    DataGridView dgvPasswords = null;
                    foreach (Control control in contentPanel.Controls)
                    {
                        if (control is DataGridView dgv)
                        {
                            dgvPasswords = dgv;
                            break;
                        }
                    }

                    if (dgvPasswords != null)
                    {
                        // Clear and reload the data
                        dgvPasswords.Rows.Clear();
                        var passwords = DatabaseHelper.GetStoredPasswords(userId);
                        foreach (var pwd in passwords)
                        {
                            dgvPasswords.Rows.Add(pwd.Id, pwd.Website, pwd.Username, "********");
                        }
                    }

                    MessageBox.Show("Password saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving password: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void BtnSettings_Click(object sender, EventArgs e)
        {
            contentPanel.Controls.Clear();
            Label titleLabel = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            contentPanel.Controls.Add(titleLabel);
            // Add settings functionality
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LoginForm loginForm = new LoginForm();
                this.Hide();
                loginForm.Show();
                this.Close();
            }
        }
    }
}
