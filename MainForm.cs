using static SciChain.Blockchain;
using NetCoreServer;
using System.Transactions;
using Gtk;
using Pango;
using System.Reflection;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using SciChain;
namespace SciExchange
{
    public partial class MainForm : Window
    {
        #region Properties

        /// <summary> Used to load in the glade file resource as a window. </summary>
        private Builder _builder;
        private static MainForm form;
        private static string GUID;
        private const string btcAddress = "bc1qmzhrvny0y4guhgk9kfxkez7t8j36sze8q6l889";
        private const string ethAddress = "0x04ACF5E365FE0d10c11c3a5fB79c78b64b26B0ac";
        private const string ethAPI = "U3HYWJT342P7X75MCTQUDZ5C5QDEIDKD3H";
        private const string solAddress = "FooEH59N85vFTyktDz9kB7SBmLTgB6JS8UJGHKwgrbk6";
        private Blockchain.Wallet wallet;
        public string peer = "92.205.238.105";
#pragma warning disable 649
        [Builder.Object]
        private Label balanceLabel;
        [Builder.Object]
        private Entry btcTransactionBox;
        [Builder.Object]
        private Entry ethTransactionBox;
        [Builder.Object]
        private Entry solTransactionBox;
        [Builder.Object]
        private Entry passwordBox;
        [Builder.Object]
        private Button loginBut;
        [Builder.Object]
        private Button copyBut;
        [Builder.Object]
        private Button copyETHBut;
        [Builder.Object]
        private Button copySOLBut;
        [Builder.Object]
        private Button swapBut;
        [Builder.Object]
        private Label statusLabel;
        [Builder.Object]
        private Notebook tabsView;
#pragma warning restore 649

        #endregion

        #region Constructors / Destructors


        /// <summary>
        /// The function creates a MainForm object using a Builder to load a Glade file.
        /// </summary>
        /// <returns>
        /// An instance of the `MainForm` class is being returned.
        /// </returns>
        public static MainForm Create()
        {
            Builder builder = new Builder(new FileStream(System.IO.Path.GetDirectoryName(Environment.ProcessPath) + "/" + "Glade/MainForm.glade", FileMode.Open));
            return new MainForm(builder, builder.GetObject("mainform").Handle);
        }

        /// <summary> Specialised constructor for use only by derived class. </summary>
        /// <param name="builder"> The builder. </param>
        /// <param name="handle">  The handle. </param>
        protected MainForm(Builder builder, IntPtr handle) : base(handle)
        {
            _builder = builder;
            builder.Autoconnect(this);
            SetupHandlers();
            form = this;
        }

        #endregion

        #region Handlers

        /// <summary> Sets up the handlers. </summary>
        protected void SetupHandlers()
        {
            swapBut.Clicked += SwapBut_Clicked;
            loginBut.Clicked += loginBut_Click;
            swapBut.Clicked += SwapBut_Clicked;
            copyBut.Clicked += CopyBut_Clicked;
            copyETHBut.Clicked += CopyETHBut_Clicked;
            copySOLBut.Clicked += CopySOLBut_Clicked;
            this.Destroyed += MainForm_Destroyed;
        }

        private void MainForm_Destroyed(object? sender, EventArgs e)
        {
            Save();
            wallet.Save(passwordBox.Text);
            Application.Quit();
        }

        private void CopySOLBut_Clicked(object? sender, EventArgs e)
        {
            TextCopy.ClipboardService.SetText(solAddress);
        }

        private void CopyETHBut_Clicked(object? sender, EventArgs e)
        {
            TextCopy.ClipboardService.SetText(ethAddress);
        }

        private void SwapBut_Clicked(object? sender, EventArgs e)
        {
            if (tabsView.CurrentPage == 0)
            {
                Block.Transaction tr = new Block.Transaction(Block.Transaction.Type.transaction, GUID, wallet.PublicKey, GUID, 0);
                tr.Data = btcTransactionBox.Text;
                tr.SignTransaction(wallet.PrivateKey);
                AddTransaction(tr);
            }
            else if (tabsView.CurrentPage == 1)
            {
                Block.Transaction tr = new Block.Transaction(Block.Transaction.Type.transaction, GUID, wallet.PublicKey, GUID, 0);
                tr.Data = ethTransactionBox.Text;
                tr.SignTransaction(wallet.PrivateKey);
                AddTransaction(tr);
            }
            else if (tabsView.CurrentPage == 2)
            {
                Block.Transaction tr = new Block.Transaction(Block.Transaction.Type.transaction, GUID, wallet.PublicKey, GUID, 0);
                tr.Data = solTransactionBox.Text;
                tr.SignTransaction(wallet.PrivateKey);
                AddTransaction(tr);
            }
        }

        private void loginBut_Click(object? sender, EventArgs e)
        {
            wallet = new Blockchain.Wallet();
            wallet.Load(passwordBox.Text);
            string st = RSA.RSAParametersToStringAll(wallet.PrivateKey);
            GUID = CalculateHash(st);
            Initialize(wallet);
            ChatClient cl = new ChatClient(peer, 8333);
            cl.ConnectAsync();
            ConnectToPeer(peer, cl, 8333);
            Blockchain.Load();
            StartTimer();
            statusLabel.Text = "Logged In:" + GUID;
            balanceLabel.Text = "Balance: " + GetBalance(GUID).ToString();
        }

        /// <summary>
        /// The function `CopyBut_Clicked` copies the value of the `GUID` variable to the clipboard when
        /// a button is clicked.
        /// </summary>
        /// <param name="sender">The `sender` parameter in the `CopyBut_Clicked` method refers to the
        /// object that raised the event. In this case, it would be the button that was clicked to
        /// trigger the event.</param>
        /// <param name="EventArgs">The `EventArgs` parameter in the `CopyBut_Clicked` event handler
        /// method is a base class for classes containing event data. It is often used when the event
        /// handler does not need to pass any additional information about the event.</param>
        private void CopyBut_Clicked(object? sender, EventArgs e)
        {
            TextCopy.ClipboardService.SetText(btcAddress);
        }

        #endregion



        /// <summary>
        /// The Timer function updates status, balance, and reputation labels on a form at regular
        /// intervals.
        /// </summary>
        private static async void Timer()
        {
            do
            {
                try
                {
                    form.balanceLabel.Text = "Balance: " + GetBalance(GUID).ToString();
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            } while (true);

        }
        /// <summary>
        /// The StartTimer function creates a new thread to run the Timer method.
        /// </summary>
        private void StartTimer()
        {
            Thread th = new Thread(Timer);
            th.Start();
        }

    }
}