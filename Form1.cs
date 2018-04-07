using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyNetworking;
using EasyNetworking.IRC;

namespace IRC_client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.FormClosing += Form1_FormClosing;
            textBox1.KeyDown += HandleInput;

            Manager.ChannelJoin += OnChannelJoin;
            Manager.DataReceived += OnDataReceived;
            Manager.MessageReceived += OnMessageReceived;

            Manager.SetNick("TestName111");
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Manager.Disconnect();
        }

        private void OnMessageReceived(object sender, IRC_EventArgs e)
        {
            if (e.Recipient == Manager.Nick)
            {
                AddText(string.Format("<{0}> {1}", e.User.Nick, e.Message), null, e.User.Nick);
            }
            else
            {
                AddText(string.Format("<{0}> {1}", e.User.Nick, e.Message), null, e.Recipient);
            }
        }

        private void Suppress(TextBox textBox, KeyEventArgs e)
        {
            textBox.Clear();
            e.SuppressKeyPress = true;
        }

        private void OnDataReceived(object sender, Socket_EventArgs e)
        {
            AddText(e.Data.String);
        }

        private void _AddText(RichTextBox richText, string str, Color? col = null)
        {
            if (richText == null)
                return;

            if (col != null)
            {
                if (richText.InvokeRequired)
                {
                    richText.Invoke(new MethodInvoker(delegate
                        {
                            richText.SelectionColor = (Color)col;
                        }));
                }
                else
                {
                    richText.SelectionColor = (Color)col;
                }
            }

            if (richText.InvokeRequired)
            {
                richText.Invoke(new MethodInvoker(delegate
                    {
                        richText.AppendText(str);
                    }));
            }
            else
            {
                richText.AppendText(str);
            }
        }

        private void AddText(string str, Color? col = null, string tab = "Status")
        {
            if (!str.EndsWith("\n"))
            {
                str += "\n";
            }

            if (tab != "Status")
            {
                TabPage _tab = FindOrCreate(tab);
                RichTextBox richText = (RichTextBox)_tab.Controls[tab + "text"];

                _AddText(richText, str, col);
            }
            else
            {
                _AddText(richTextBox1, str, col);
            }
        }

        private void OnChannelJoin(object sender, IRC_EventArgs e)
        {
            if (e.Succesful)
            {
                FindOrCreate(e.Recipient);
            }
            else
            {
                string str = "Unknown";

                if (Enum.IsDefined(typeof(IRC_CHANNEL_ERROR), e.Flag))
                {
                    switch ((IRC_CHANNEL_ERROR)e.Flag)
                    {
                        case IRC_CHANNEL_ERROR.BANNED:
                            str = "Banned from channel";
                            break;
                        case IRC_CHANNEL_ERROR.FULL:
                            str = "Channel full";
                            break;
                        case IRC_CHANNEL_ERROR.INVITE_ONLY:
                            str = "Channel is invite-only";
                            break;
                        case IRC_CHANNEL_ERROR.WRONG_PASSWORD:
                            str = "Wrong password";
                            break;
                        case IRC_CHANNEL_ERROR.TOO_MANY:
                            str = "Too many channels";
                            break;
                    }
                }

                AddText(string.Format("Can't join {0}! Reason: {1}", e.Recipient, str), Color.Red);
            }
        }

        public void HandleInput(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox textBox = (TextBox)sender;
                string[] temp = textBox.Text.Split(' ');
                string cmd = temp[0].ToLower();

                if (cmd.StartsWith("/"))
                {
                    if (temp.Length == 2)
                    {
                        switch (cmd)
                        {
                            case "/join":
                                {
                                    Manager.JoinChannel(temp[1]);
                                    break;
                                }
                            case "/query":
                                {
                                    
                                    break;
                                }
                            case "/nick":
                                {
                                    Manager.SetNick(temp[1]);
                                    break;
                                }
                        }
                    }
                    else if (temp.Length == 3)
                    {
                        switch (cmd)
                        {
                            case "/join":
                                {
                                    Manager.JoinChannel(temp[1], temp[2]);
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    if (textBox.Name.EndsWith("input"))
                    {
                        string recpt = textBox.Name.Replace("input", "");

                        Manager.SendMessage(recpt, textBox.Text);
                        AddText(string.Format("<{0}> {1}", Manager.Nick, textBox.Text), null, recpt);
                    }
                }

                Suppress(textBox, e);
            }
        }

        public TabPage CreateTab(string name)
        {
            TabPage page = new TabPage();
            page.Text = name;

            RichTextBox richText = new RichTextBox();
            richText.ReadOnly = true;
            richText.Dock = DockStyle.Fill;
            richText.Name = name + "text";

            page.Controls.Add(richText);

            if (name.StartsWith("#"))
            {
                ListBox listBox = new ListBox();
                listBox.Items.Clear();
                listBox.Dock = DockStyle.Right;
                listBox.IntegralHeight = false;
                listBox.Name = name + "list";

                page.Controls.Add(listBox);
            }

            TextBox textBox = new TextBox();
            textBox.KeyDown += HandleInput;
            textBox.Dock = DockStyle.Bottom;
            textBox.Name = name + "input";

            page.Controls.Add(textBox);

            if (tabControl1.InvokeRequired)
            {
                tabControl1.Invoke(new MethodInvoker(delegate
                    {
                        tabControl1.TabPages.Add(page);
                        tabControl1.SelectTab(page);
                    }));
            }
            else
            {
                tabControl1.TabPages.Add(page);
                tabControl1.SelectTab(page);
            }

            return page;
        }

        public TabPage FindOrCreate(string name)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                if (page.Name.ToLower() == name.ToLower())
                {
                    return page;
                }
            }

            return CreateTab(name);
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerConnect form = new ServerConnect();
            form.ShowDialog();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog form = new SettingsDialog();
            form.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
