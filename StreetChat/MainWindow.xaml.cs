using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Themes;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Media;

namespace StreetChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPEndPoint FromIPEndPoint = new IPEndPoint(new IPAddress(0), (0));
        private UdpClient udpclient;
        private TcpListener tcpListener;
        private Thread threadUdp;
        private Thread threadTcp;
        private System.Collections.ObjectModel.ObservableCollection<User> users = new System.Collections.ObjectModel.ObservableCollection<User>();
        private bool newMsgRecieved = false;
        private int AdminPassHashed = -1507298789;
        private User localUser = new User(Dns.GetHostName());
        private string rightClickedUserName = "";
        private int portNumber = 5000;
        private string LastPrivate = "";
        private List<IPAddress> localIPs = new List<IPAddress>();
        private List<IPAddress> broadcastIPs = new List<IPAddress>();
        private bool ChatFocused = true;
        private List<string> LastMessages = new List<string>();
        private int currentLastIndex = -1;
        private ResourceManager lang;
        private System.Windows.Threading.DispatcherTimer tmrNewMessage = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            CultureInfo ci = CultureInfo.CurrentUICulture;

            if (ci.TwoLetterISOLanguageName == "da")
            {
                cmbbxLanguage.SelectedIndex = 1;
                lang = new ResourceManager("StreetChat.lang.da-DK", Assembly.GetExecutingAssembly());
            }
            else
            {
                cmbbxLanguage.SelectedIndex = 0;
                lang = new ResourceManager("StreetChat.lang.en-GB", Assembly.GetExecutingAssembly());
            }

            try
            {
                udpclient = new UdpClient(portNumber);
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format(lang.GetString("PortInUse"), portNumber.ToString()));
                Process.GetCurrentProcess().Kill();
                throw;
            }

            cntxtKick.IsEnabled = false;

            IPHostEntry iphostentry = Dns.GetHostEntry(localUser.Username);

            this.DataContext = users;
            //threadUdp = new Thread(new ThreadStart(listenForPeers));
            //threadUdp.Start();

            //tcpListener = new TcpListener(IPAddress.Any, portNumber);
            //threadTcp = new Thread(new ThreadStart(ListenForTcpConnection));
            //threadTcp.Start();

            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    broadcastIPs.Add(Utilities.GetBroadcastAddress(ipaddress));
                    localIPs.Add(ipaddress);
                    localUser.IPAddress = ipaddress;
                }
            }

            localUser.ChatVersion = Assembly.GetExecutingAssembly().GetName().Version;
            localUser.IPEndPoint = new IPEndPoint(localUser.IPAddress, portNumber);
            localUser.IsLocalUser = true;
            localUser.UniqueID = Guid.NewGuid();

            SetControlText();
            users.Add(localUser);
            //UpdateUserPanel();
            rtxtbxChat.AppendText(GetText("WelcomeString"));

            //Create timer to send Echo
            System.Windows.Threading.DispatcherTimer tmrSendEcho = new System.Windows.Threading.DispatcherTimer();
            tmrSendEcho.Tick += new EventHandler(tmrSendEcho_Tick);
            tmrSendEcho.Interval = new TimeSpan(0, 0, 5);
            tmrSendEcho.Start();

            ////Create timer to clean up users
            //System.Windows.Threading.DispatcherTimer tmrClearUpUsers = new System.Windows.Threading.DispatcherTimer();
            //tmrClearUpUsers.Tick += new EventHandler(tmrClearUpUsers_Tick);
            //tmrClearUpUsers.Interval = new TimeSpan(0, 0, 5);
            //tmrClearUpUsers.Start();
            
            //Set settings for tmrNewMessage
            tmrNewMessage.Tick += new EventHandler(tmrNewMessage_Tick);
            tmrNewMessage.Interval = new TimeSpan(0, 0, 1);
        }

        //private void tmrClearUpUsers_Tick(object sender, EventArgs e)
        //{
        //    CleanUpUsers();
        //}
        ///// <summary>
        ///// Checks each user if he/she is connected. If not removes user.
        ///// </summary>
        //private void CleanUpUsers()
        //{
        //    foreach (User _user in users.FindAll(delegate(User userToFind) { return !userToFind.IsLocalUser && !userToFind.IsConnected; }))
        //    {
        //        _user.tcpClient.Close();
        //        users.Remove(_user);
        //        WriteSystemText(String.Format(lang.GetString("LoggedOff"), _user.Username));
        //    }

        //    UpdateUserPanel();
        //}
        /// <summary>
        /// Clears the UserPanel and then adds all users again from the updated users-list.
        /// </summary>
        private void UpdateUserPanel()
        {
            
        }
        private void tmrNewMessage_Tick(object sender, EventArgs e)
        {
            if (Title == GetText("Title")) { Title = GetText("TitleNewMessage"); }
            else { Title = GetText("Title"); }
        }

        private void tmrSendEcho_Tick(object sender, EventArgs e)
        {
            SendEcho();
        }
        /// <summary>
        /// Sends out an UDP echo to look for other peers
        /// </summary>
        private void SendEcho()
        {
            try
            {
                byte[] send = System.Text.Encoding.Unicode.GetBytes("ECHO");

                foreach (IPAddress address in broadcastIPs)
                {
                    udpclient.Send(send, send.Length, new IPEndPoint(address, portNumber));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("You are not part of a NETWORK");
            }
        }
        /// <summary>
        /// Gets string from language-resource-file
        /// </summary>
        /// <param name="StringName"></param>
        /// <returns></returns>
        private string GetText(string StringName)
        {
            return lang.GetString(StringName).Replace(Environment.NewLine, "\r");
        }

        /// <summary>
        /// Writes a Systemmessage in the chat in red.
        /// <param name="text">Text to write</param>
        /// </summary>
        private void WriteSystemText(string text)
        {
            WriteText(text, Brushes.Red);
        }
        /// <summary>
        /// Writes a new line of text in the chat. Fx
        /// Lol omg!
        /// <param name="text">Text to write</param>
        /// </summary>
        private void WriteText(string newText)
        {
            WriteText(newText, Brushes.Black);
        }
        private void WriteText(string newText, Brush color)
        {
            TextRange text = new TextRange(rtxtbxChat.Document.ContentEnd, rtxtbxChat.Document.ContentEnd);
            text.Text = "\r" + newText;
            text.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private void btnSendMsg_Click(object sender, RoutedEventArgs e)
        {
            string message = Regex.Replace(txtbxMsg.Text, @"\s+", " ");

            if (!(message == ""))
            {
                if (message[0] == '/')
                {
                    message += "      ";

                    switch (message.Substring(1, message.IndexOf(" ") - 1).ToLower())
                    {
                        case ("admins"):
                            if (AdminExists())
                            {
                                WriteSystemText(lang.GetString("FollowingIsAdmin"));
                                foreach (User admin in users.Where(X => X.IsAdmin))
                                {
                                    WriteSystemText(admin.Username);
                                }
                                WriteSystemText("----------------");
                            }
                            else
                            {
                                WriteSystemText(lang.GetString("NoAdmin"));
                            }
                            break;
                        case ("afk"):
                            if (localUser.Status != "AFK")
                            {
                                localUser.Status = "AFK";
                                SendMessage(MessageType.System, "SAFK");
                                WriteSystemText(String.Format(lang.GetString("LocalStatusChange"), "AFK"));
                            }
                            else
                            {
                                localUser.Status = "";
                                SendMessage(MessageType.System, "S");
                                WriteSystemText(String.Format(lang.GetString("LocalStatusRemoved"), "AFK"));
                            }
                            break;
                        case ("help"):
                            ShowHelp();
                            break;
                        case ("info"):
                            WriteSystemText(lang.GetString("Info"));
                            break;
                        case ("ip"):
                            SendMessage(MessageType.System, "I");
                            WriteSystemText(String.Format(lang.GetString("ShowIP"), localUser.Username, localUser.IPAddress.ToString()));
                            break;
                        case ("ips"):
                            WriteSystemText(lang.GetString("ShowIPs"));
                            foreach (User User in users)
                            {
                                WriteSystemText(User.Username + " - " + User.IPAddress.ToString());
                            }
                            WriteSystemText("---------------");
                            break;
                        case ("login"):
                            if (localUser.IsAdmin)
                            {
                                WriteSystemText(lang.GetString("AlreadyAdmin"));
                            }
                            else
                            {
                                if (message.Substring(7).Trim().GetHashCode() == AdminPassHashed)
                                {
                                    ChangeToAdmin();
                                }
                                else
                                {
                                    localUser.IsAdmin = false;
                                    cntxtKick.IsEnabled = false;
                                    WriteSystemText(lang.GetString("WrongLogin"));
                                }
                            }
                            break;
                        case ("kick"):
                            if (localUser.IsAdmin)
                            {
                                if (message.Substring(6, 1) == " ")
                                {
                                    WriteSystemText(lang.GetString("MissingUsername"));
                                }
                                else if (getUser(message.Trim().Substring(6)) == null)
                                {
                                    WriteSystemText(String.Format(lang.GetString("UserNotOnline"), message.Trim().Substring(6)));
                                }
                                else
                                {
                                    KickUser(message.Substring(6).Trim());
                                }
                            }
                            else
                            {
                                WriteSystemText(lang.GetString("NotPossible"));
                            }
                            break;
                        case ("love"):
                            WriteText(lang.GetString("Love"), Brushes.DeepPink);
                            this.Title = "<3 Henriette <3";
                            break;
                        case ("request"):
                            if (!localUser.IsAdmin)
                            {

                                if (users.Count == 1)
                                {
                                    WriteSystemText(lang.GetString("AloneAdminRequest"));
                                }
                                else
                                {
                                    if (!AdminExists())
                                    {
                                        ChangeToAdmin();
                                    }
                                    else
                                    {
                                        WriteSystemText(lang.GetString("AdminAlreadyExist"));
                                    }
                                }
                            }
                            else
                            {
                                WriteSystemText(lang.GetString("AlreadyAdmin"));
                            }

                            break;
                        case ("status"):
                            string newStatus = message.Substring(8).Trim();
                            string oldStatus = localUser.Status;

                            if (oldStatus == newStatus)
                            {
                                //Breaks case if new and old status is the same. Case-sencitive
                                break;
                            }

                            if (newStatus.Length == 0)
                            {
                                if (oldStatus.Length != 0)
                                {
                                    localUser.Status = "";
                                    SendMessage(MessageType.System, "S");
                                    WriteSystemText(String.Format(lang.GetString("LocalStatusRemoved"), oldStatus));
                                }
                            }
                            else if (newStatus.Length > 5)
                            {
                                WriteSystemText(lang.GetString("WrongStatusFormat"));
                            }
                            else
                            {
                                localUser.Status = newStatus;
                                SendMessage(MessageType.System, "S" + newStatus);
                                WriteSystemText(String.Format(GetText("LocalStatusChange"), newStatus));
                            }
                            break;
                        case ("username"):
                            string newUsername = message.Substring(10).Trim();
                            Regex regexItem = new Regex(@"^[a-zA-Z0-9_-]{3,20}$");

                            if (!regexItem.IsMatch(newUsername))
                            {
                                WriteSystemText(lang.GetString("WrongUsernameFormat"));
                            }
                            else if (UserExists(newUsername))
                            {
                                if (newUsername == localUser.Username)
                                {
                                    WriteSystemText(String.Format(lang.GetString("UsernameExist1"), newUsername));
                                }
                                else
                                {
                                    WriteSystemText(String.Format(lang.GetString("UsernameExist2"), newUsername));
                                }
                            }
                            else
                            {
                                SendMessage(MessageType.System, "U" + newUsername);
                                users[users.IndexOf(localUser)].Username = newUsername;
                                localUser.Username = newUsername;
                                UpdateUserPanel();
                                WriteSystemText(String.Format(lang.GetString("LocalUsernameChange"), newUsername));
                            }
                            break;
                        case ("versions"):
                            WriteSystemText(lang.GetString("ShowVersions"));
                            foreach (User User in users)
                            {
                                WriteSystemText(User.Username + " - " + User.ChatVersion);
                            }
                            WriteSystemText("------------------");
                            break;
                        case ("w"):
                            string username = message.Split(new char[] { ' ' })[1];
                            User userToWhisper = getUser(username);
                            string newMessage = message.Substring(message.IndexOf(' ', 3) + 1).Trim();

                            if (username == "")
                            {
                                WriteSystemText(lang.GetString("MissingUsername"));
                            }
                            else if (newMessage == "")
                            {
                                WriteSystemText(lang.GetString("EmptyMessage"));
                            }
                            else
                            {
                                if (userToWhisper != null)
                                {
                                    if (userToWhisper.IsLocalUser)
                                    {
                                        WriteSystemText(lang.GetString("PrivateToLocalError"));
                                    }
                                    else
                                    {
                                        SendMessageToPeer(userToWhisper.tcpClient, MessageType.Private, newMessage);
                                        WriteText(">" + userToWhisper.Username + ": " + newMessage, Brushes.MediumPurple);
                                        LastPrivate = userToWhisper.Username;
                                    }
                                }
                                else
                                {
                                    WriteSystemText(String.Format(lang.GetString("UserNotOnline"), username));
                                }
                            }
                            break;
                        default:
                            WriteSystemText(lang.GetString("WrongCommand"));
                            break;
                    }
                }
                else
                {
                    SendMessage(message);
                    WriteText(localUser.Username + ": " + message.Replace("%user%", localUser.Username));
                }

                LastMessages.Insert(0, txtbxMsg.Text);
                currentLastIndex = -1;
                txtbxMsg.Text = "";
                FocusChatFrame();
            }
        }

        /// <summary>
        /// Show help
        /// </summary>
        private void ShowHelp()
        {
            WriteSystemText(GetText("Help"));
        }

        /// <summary>
        /// Change the current user to Admin
        /// </summary>
        private void ChangeToAdmin()
        {
            WriteSystemText(String.Format(lang.GetString("IsNowAdmin"), localUser.Username));
            users.Where(X => X.IsLocalUser).FirstOrDefault().IsAdmin = true;
            localUser.IsAdmin = true;
            cntxtKick.IsEnabled = true;
            SendMessage(MessageType.System, "AL");
        }

        private bool UserExists(string username)
        {
            return users.Any(X => X.Username == username);
        }
        /// <summary>
        /// Sends a message.
        /// <param name="message">Message to send</param>
        /// </summary>
        private void SendMessage(string message)
        {
            foreach (User peer in users.Where(X => !X.IsLocalUser && X.IsConnected))
            {
                SendMessageToPeer(peer.tcpClient, MessageType.Normal, message);
            }
        }
        /// <summary>
        /// Sends a message.
        /// <param name="type">The message type</param>
        /// <param name="message">Message to send</param>
        /// </summary>
        private void SendMessage(MessageType type, string message)
        {
            foreach (User peer in users.Where(X => !X.IsLocalUser && X.IsConnected))
            {
                SendMessageToPeer(peer.tcpClient, type, message);
            }
        }
        /// <summary>
        /// Sends a message to a specific TcpClient.
        /// <param name="peer">The TcpClient to send the message to</param>
        /// <param name="message">Message to send</param>
        /// </summary>
        private void SendMessageToPeer(TcpClient peer, string message)
        {
            SendMessageToPeer(peer, MessageType.Normal, message);
        }
        /// <summary>
        /// Sends a message to a specific TcpClient.
        /// <param name="peer">The TcpClient to send the message to</param>
        /// <param name="type">Type of the message. Fx 'M' for message</param>
        /// <param name="attachment">The object to attach</param>
        /// </summary>
        private void SendMessageToPeer(TcpClient peer, MessageType type, object attachment)
        {
            NetworkStream clientStream = peer.GetStream();
            //UnicodeEncoding encoder = new UnicodeEncoding();
            //byte[] buffer = encoder.GetBytes(message);
            byte[] buffer = Utilities.ObjectToByteArray(new Message(localUser.UniqueID, type, attachment));

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
        private bool AdminExists()
        {
            return users.Any(X => X.IsAdmin == true);
        }
        private void FocusChatFrame()
        {
            txtbxMsg.Focus();
        }
        private void SetControlText()
        {
            cntxtSeeIP.Header = GetText("SeeIP");
            cntxtSeeIP.ToolTip = GetText("SeeIPTT");
            cntxtSeeVersion.Header = GetText("SeeVersion");
            cntxtSeeVersion.ToolTip = GetText("SeeVersionTT");
            cntxtWriteTo.Header = GetText("WriteTo");
            cntxtWriteTo.ToolTip = GetText("WriteToTT");
            cntxtKick.Header = GetText("Kick");
            cntxtKick.ToolTip = GetText("KickTT");
            cntxtCopyAll.Header = GetText("CopyAll");
            cntxtCopyTxt.Header = GetText("CopyText");
            cntxtDelAll.Header = GetText("DelAll");
            chkbxNotify.Content = GetText("NotifyNewMessage");
            lblBottomText.Content = String.Format(GetText("BottomText"), localUser.ChatVersion);
        }
        /// <summary>
        /// Gets a specific user from the users-list with the Guid as criteria.
        /// Returns null if no user is found
        /// <param name="uniqueID">The guid to find</param>
        /// </summary>
        private User getUser(Guid uniqueID)
        {
            User userToGet = users.Where(X => X.UniqueID == uniqueID).FirstOrDefault();

            return userToGet;
        }
        /// <summary>
        /// Gets a specific user from the users-list with the username as criteria.
        /// Returns null if no user is found
        /// <param name="userIP">The IPAddress to find</param>
        /// </summary>
        private User getUser(string username)
        {
            User userToGet = users.Where(X => X.Username == username).FirstOrDefault();

            return userToGet;
        }
        private void playSound(string soundToPlay)
        {
            if (soundToPlay == "newMsg")
            {
                try
                {
                    SoundPlayer sndplayr = new SoundPlayer(Properties.Resources.SpeechOn);
                    sndplayr.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ": " + ex.StackTrace.ToString(), "Error");
                }
            }
        }
        /// <summary>
        /// Enum for messagetypes
        /// </summary>
        public enum MessageType
        {
            Normal,
            System,
            Info,
            Private
        };

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CloseChat();
        }
        private void CloseChat()
        {
            //foreach (User _user in users.FindAll(delegate(User userToFind) { return !userToFind.IsLocalUser && userToFind.IsConnected; }))
            //{
            //    _user.tcpClient.GetStream().Close();
            //    _user.tcpClient.Close();
            //}
            //threadUdp.Abort();
            //udpclient.Close();
            //Process.GetCurrentProcess().Kill();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            ChatFocused = true;
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            ChatFocused = false;
        }

        private void lstbxUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstbxUsers.SelectedIndex != -1)
            {
                string insertText = "";
                string username = ((User)lstbxUsers.SelectedItem).Username;
                if (txtbxMsg.Text == "")
                {
                    insertText = "/w " + username + " ";
                }
                else
                {
                    if (txtbxMsg.Text.EndsWith(" "))
                    {
                        insertText = username + " ";
                    }
                    else
                    {
                        insertText = " " + ((User)lstbxUsers.SelectedItem).Username + " ";
                    }
                }
                txtbxMsg.Text += insertText;
                //FocusChatFrame();
            }
        }

        private void lstbxUsers_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstbxUsers.SelectedIndex != -1)
            {
                rightClickedUserName = ((User)lstbxUsers.SelectedItem).Username;
            }
        }

        private void lstbxUsers_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lstbxUsers.SelectedIndex != -1)
            {
                string clickedUsername = ((User)lstbxUsers.SelectedItem).Username;

                cntxtSeeIP.ToolTip = GetText("SeeIPTT").Replace("%user%", clickedUsername);
                cntxtSeeVersion.ToolTip = GetText("SeeVersionTT").Replace("%user%", clickedUsername);
                cntxtWriteTo.ToolTip = GetText("WriteToTT").Replace("%user%", clickedUsername);
                cntxtKick.ToolTip = GetText("KickTT").Replace("%user%", clickedUsername);
            }
        }

        private void cntxtSeeIP_Click(object sender, RoutedEventArgs e)
        {
            //Fires when user clicks on SeeIP in contextmenu
            WriteSystemText(String.Format(GetText("ShowIP"), ((User)lstbxUsers.SelectedItem).Username, ((User)lstbxUsers.SelectedItem).IPAddress.ToString()));
        }

        private void cntxtSeeVersion_Click(object sender, RoutedEventArgs e)
        {
            //Fires when user clicks on SeeVersion in contextmenu
            WriteSystemText(String.Format(GetText("ShowVersion"), ((User)lstbxUsers.SelectedItem).Username, ((User)lstbxUsers.SelectedItem).ChatVersion));
        }

        private void cntxtWriteTo_Click(object sender, RoutedEventArgs e)
        {
            //Fires when user clicks on WriteTo in contextmenu
            txtbxMsg.Text = "/w " + ((User)lstbxUsers.SelectedItem).Username + " " + txtbxMsg.Text;
        }

        private void cntxtKick_Click(object sender, RoutedEventArgs e)
        {
            //Fires when user clicks on Kick in contextmenu
            KickUser(((User)lstbxUsers.SelectedItem).Username);
        }
        private void KickUser(string Username)
        {
            SendMessageToPeer(getUser(Username).tcpClient, MessageType.System, "AK");
            SendMessage(MessageType.System, "AIK" + Username);
            WriteSystemText(String.Format(lang.GetString("UserKicked"), Username, localUser.Username));
        }
    }
}